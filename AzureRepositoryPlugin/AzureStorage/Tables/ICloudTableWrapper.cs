using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windward.Hub.StorageContract.Filtering;

namespace AzureStorage.Tables
{
    public interface ICloudTableWrapper<T> where T : ITableEntity, new()
    {
        Task Init();
        Task<T[]> GetEntities(FilterGroup filter, int numResults = 0, string prevRowKey = null, string partitionKey = null, string sortColumn = null, bool sortOrderDescending = false);
        Task<T[]> GetEntities(string filter, int numResults = 0, string prevRowKey = null, string partitionKey = null, string sortColumn = null, bool sortOrderDescending = false);
        Task<T> GetSingleEntity(string filter);
        Task<TChild> GetSingleEntity<TChild>(string filter) where TChild : T, new();
        Task<TableResult> ReplaceEntity(T entity);
        Task<TableResult> UpsertEntity(T entity);
        Task<TableResult> DeleteEntity(T entity);
        Task<TableBatchResult[]> DeleteEntities(T[] entities);
    }
}
