using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Windward.Hub.StorageContract.Filtering;

namespace AzureStorage.Tables
{
    public class CloudTableWrapper<T> : ICloudTableWrapper<T> where T : ITableEntity, new()
    {
        private CloudTable _table;

        private readonly string _storageConnectionString;
        private readonly string _tableName;

        internal CloudTableWrapper(string storageConnectionString, string tableName)
        {
            _storageConnectionString = storageConnectionString;
            _tableName = tableName;
        }

        public async Task Init()
        {
            await InitTable();
        }

        protected async Task InitTable()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(_storageConnectionString);
            CloudTableClient client = account.CreateCloudTableClient();
            _table = client.GetTableReference(_tableName);
            await _table.CreateIfNotExistsAsync();
        }

        public async Task<T[]> GetEntities(FilterGroup filter, int numResults = 0, string prevRowKey = null,
            string partitionKey = null, string sortColumn = null, bool sortOrderDescending = false)
        {
            string filterString = CloudTableUtils.BuildFilterString(filter);
            return await GetEntities(filterString, numResults, prevRowKey, partitionKey, sortColumn,
                sortOrderDescending);
        }

        public async Task<T[]> GetEntities(string filter, int numResults = 0, string prevRowKey = null, string partitionKey = null, string sortColumn = null, bool sortOrderDescending = false)
        {
            TableQuery<T> query = new TableQuery<T>().Where(filter);

            // we can only use query.take() to narrow down to the correct number of results if a sort order is not provided because AzureTables doesn't support OrderBy, so we need all results to handle sorting locally.
            if (numResults > 0 && sortColumn == null)
            {
                // we take one extra result when a prev row key is provided because we will end up grabbing one row from the DB that we already have.
                query = prevRowKey == null ? query.Take(numResults) : query.Take(numResults + 1);
            }

            
            TableContinuationToken token = default;
            // we can only grab results starting at a certain row if a continuation key is not provided because AuzreTables doesn't support OrderBy, so we need all results to handle sorting locally.
            if (prevRowKey != null && sortColumn == null)
            {
                if (partitionKey == null)
                {
                    throw new ArgumentException("If a prev row key is provided, a partition key must also be provided.");
                }
                token = new TableContinuationToken();
                token.NextRowKey = prevRowKey; // bug bug might cause off by one error (returning first element twice
                token.NextPartitionKey = partitionKey;
            }



            var ret = new List<T>();
            do
            {
                TableQuerySegment<T> segment = await _table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                ret.AddRange(segment.Results);
            } while (token != null);

            // we do sorting locally because azure tables doesn't support OrderBy queries (super lame)
            if (sortColumn != null)
            {
                PropertyInfo prop = null;
                PropertyInfo[] props = typeof(T).GetProperties();

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (PropertyInfo propertyInfo in props)
                {
                    string propName = propertyInfo.Name;
                    if (sortColumn.Equals(propName))
                    {
                        prop = propertyInfo;
                        break;
                    }
                }

                if (prop != null)
                {
                    object KeySelector(T entity) => prop.GetValue(entity);
                    ret = sortOrderDescending ? ret.OrderByDescending(KeySelector).ToList() : ret.OrderBy(KeySelector).ToList();
                }
                else
                {
                    throw new ArgumentException($"Unable to find property matching sortCollumn {sortColumn}");
                }
            }

            // in this case we were unable to use a continuation token, so we need to ignore the first part of the list leading up to the continuation token.
            if (sortColumn != null && prevRowKey != null)
            {
                int prevRowInd = ret.FindIndex(entity => entity.RowKey.Equals(prevRowKey));
                if (prevRowInd < 0)
                {
                    throw new ArgumentException($"prevRowKey {prevRowKey} not in result set");
                }

                ret.RemoveRange(0, prevRowInd + 1);
            }
            // in this case we were able to use a continuation token, so we just need to ignore the first element in the list since this will be the prevRow
            else if (prevRowKey != null)
            {
                ret.RemoveAt(0);
            }

            // don't return more than numResults
            if (numResults > 0 && numResults < ret.Count)
            {
                ret = ret.GetRange(0, numResults);
            }

            return ret.ToArray();
        }

        private async Task<TElement> QuerySingleEntity<TElement>(string filter) where TElement : ITableEntity, new()
        {
            TableQuery<TElement> query = new TableQuery<TElement>().Where(filter);
            TableContinuationToken token = default;
            var ret = new List<TElement>();
            do
            {
                TableQuerySegment<TElement> segment = await _table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                ret.AddRange(segment.Results);
            } while (token != null);

            int count = ret.Count();
            if (count == 0)
            {
                throw new Exception($"TABLE ROW NOT FOUND EXCEPTION: {filter}");
            }
            if (count > 1)
            {
                throw new Exception($"DUPLICATE TABLE ROW EXCEPTION: {filter}");
            }

            return ret[0];
        }

        public async Task<T> GetSingleEntity(string filter)
        {
            return await QuerySingleEntity<T>(filter);
        }

        public async Task<TChild> GetSingleEntity<TChild>(string filter) where TChild : T, new()
        {
            return await QuerySingleEntity<TChild>(filter);
        }

        public async Task<TableResult> ReplaceEntity(T entity)
        {
            TableOperation replace = TableOperation.Replace(entity);
            TableResult result = await _table.ExecuteAsync(replace);
            return result;
        }

        public async Task<TableResult> UpsertEntity(T entity)
        {
            TableOperation upsert = TableOperation.InsertOrReplace(entity);
            TableResult result = await _table.ExecuteAsync(upsert);
            return result;
        }

        public async Task<TableResult> DeleteEntity(T entity)
        {
            TableOperation delete = TableOperation.Delete(entity);
            TableResult result = await _table.ExecuteAsync(delete);
            return result;
        }

        public async Task<TableBatchResult[]> DeleteEntities(T[] entities)
        {
            List<TableBatchOperation> operations = new List<TableBatchOperation>();
            TableBatchOperation currentOperation = new TableBatchOperation();

            int count = 0;
            foreach (T entity in entities)
            {
                if (count == 100)
                {
                    operations.Add(currentOperation);
                    currentOperation = new TableBatchOperation();
                    count = 0;
                }
                currentOperation.Add(TableOperation.Delete(entity));
                count++;
            }
            if (count > 0)
            {
                operations.Add(currentOperation);
            }

            List<Task<TableBatchResult>> tasks = new List<Task<TableBatchResult>>();
            foreach (var op in operations)
            {
                tasks.Add(_table.ExecuteBatchAsync(op));
            }

            IEnumerable<TableBatchResult> results = await Task.WhenAll(tasks);
            return results.ToArray();
        }
    }
}
