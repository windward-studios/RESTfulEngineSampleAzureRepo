using System;
using System.Threading.Tasks;
using AzureRepositoryPlugin.AzureStorage;
using log4net;
using System.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;
using WindwardRepository;
using WindwardModels;

namespace AzureRepositoryPlugin
{
    public class AzureStorageManager
    {
        private readonly string _storageConnectionString;

        protected readonly string JobInfoTableName;

        private readonly string _templateContainer;
        private readonly string _documentContainer;
        private readonly string _docPerformanceContainer;

        private CloudTable _jobInfoTable;
        private CloudStorageAccount _storageAccount;
        private CloudTableClient _tableClient;

        private CloudBlobClient _blobClient;
        private CloudBlobContainer _jobTemplateBlob;
        private CloudBlobContainer _jobDocumentBlob;

        private readonly string _partitionKey = Guid.Empty.ToString();

        private static readonly ILog Log = LogManager.GetLogger("PluginLogger");

        public AzureStorageManager()
        {
            _storageConnectionString = ConfigurationManager.AppSettings["AzureRepositoryStorageConnectionString"];
            JobInfoTableName = ConfigurationManager.AppSettings["AzureRepositoryRestJobInfoTable"];
            _templateContainer = ConfigurationManager.AppSettings["AzureRepositoryTemplateContainer"];
            _documentContainer = ConfigurationManager.AppSettings["AzureRepositoryDocumentContainer"];
            _docPerformanceContainer = ConfigurationManager.AppSettings["AzureRepositoryDocumentPerformanceContainer"];
        }

        public void Init()
        {
            try
            {
                _storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
                _tableClient = _storageAccount.CreateCloudTableClient();

                _jobInfoTable = _tableClient.GetTableReference(JobInfoTableName);
                _jobInfoTable.CreateIfNotExists();

                _blobClient = _storageAccount.CreateCloudBlobClient();
                _jobTemplateBlob = _blobClient.GetContainerReference(_templateContainer);
                _jobTemplateBlob.CreateIfNotExists();

                _jobDocumentBlob = _blobClient.GetContainerReference(_documentContainer);
                _jobDocumentBlob.CreateIfNotExists();
            } catch(StorageException e)
            {
                Log.Error("STORAGE EXCEPTION on init: " + e);
            } catch (Exception ex)
            {
                Log.Error("EXCEPTION on init: " + ex);
            }
        }

        public async Task<bool> AddRequest(JobRequestData request)
        {
            // Add a request to storage
            JobInfoEntity entity = JobInfoEntity.FromJobRequestData(request, _partitionKey);
            entity.Status = (int)RepositoryStatus.JOB_STATUS.Pending;

            var op = TableOperation.Insert(entity);
            TableResult result = await _jobInfoTable.ExecuteAsync(op);
            bool success = result.HttpStatusCode == 204;

            if (success)
                Log.Debug($"Added request [{request.Template.Guid}] to table storage");
            else
                Log.Error($"Failed to add request [{request.Template.Guid}] to table storage: {result.HttpStatusCode}");


            // Upload template to blob storage
            await UploadBlob<Template>(request.Template, entity.JobId.ToString(), _templateContainer);

            if (success)
                Log.Debug($"Added template [{request.Template.Guid}] to blob storage");
            else
                Log.Error($"Failed to add template [{request.Template.Guid}] to blob storage: {result.HttpStatusCode}");

            return success;
        }

        public async Task<bool> UpdateRequest(Guid requestId, RepositoryStatus.JOB_STATUS newStatus)
        {
            // Update status for specific request
            JobInfoEntity entity = GetRequestInfo(requestId);
            entity.Status = (int)newStatus;

            var op = TableOperation.Replace(entity);
            TableResult result = await _jobInfoTable.ExecuteAsync(op);
            bool success = result.HttpStatusCode == 204;

            if (success)
                Log.Debug($"Updated request [{requestId}] status to {newStatus}");
            else
                Log.Error($"Failed to updat request [{requestId}] status to {newStatus}: {result.HttpStatusCode}");

            return success;
        }

        public async Task<bool> CompleteRequest<T>(Guid requestId, T generatedEntity)
        {
            // Set status to complete and add final document to blob
            JobInfoEntity entity = GetRequestInfo(requestId);

            // Upload generated document to blob
            await UploadBlob<T>(generatedEntity, entity.JobId.ToString(), _documentContainer);

            if(generatedEntity is ServiceError)
                entity.Status = (int)RepositoryStatus.JOB_STATUS.Error;
            else
                entity.Status = (int)RepositoryStatus.JOB_STATUS.Complete;

            var op = TableOperation.Replace(entity);
            TableResult result = await _jobInfoTable.ExecuteAsync(op);
            bool success = result.HttpStatusCode == 204;

            if (success)
                Log.Debug($"Updated request [{requestId}] status to Complete");
            else
                Log.Error($"Failed to update request [{requestId}] status to Complete: {result.HttpStatusCode}");

            return success;
        }

        public async Task<bool> DeleteRequest(Guid requestId)
        {
            // Remove request from storage
            JobInfoEntity entity = GetRequestInfo(requestId);
            var insertOp = TableOperation.Delete(entity);
            TableResult result = await _jobInfoTable.ExecuteAsync(insertOp);
            bool success = result.HttpStatusCode == 204;

            if (success)
                Log.Debug($"Successfully deleted request [{requestId}] from table storage");
            else
                Log.Error($"Failed to delete request [{requestId}] from table storage: {result.HttpStatusCode}");

            // Delete template if exists
            CloudBlob templateBlob = _jobTemplateBlob.GetBlobReference(requestId.ToString());
            if (templateBlob != null)
            {
                bool response = await templateBlob.DeleteIfExistsAsync();
                success &= response;

                if (success)
                    Log.Debug($"Successfully deleted request [{requestId}] from blob storage");
                else
                    Log.Error($"Failed to delete request [{requestId}] from blob storage: {result.HttpStatusCode}");
            }

            // Delete generated doc if exists
            CloudBlob documentBlob = _jobDocumentBlob.GetBlobReference(requestId.ToString());
            if (documentBlob != null)
            {
                bool response = await documentBlob.DeleteIfExistsAsync();
                success &= response;

                if (success)
                    Log.Debug($"Successfully deleted request [{requestId}] from blob storage");
                else
                    Log.Error($"Failed to delete request [{requestId}] from blob storage: {result.HttpStatusCode}");
            }

            return success;
        }

        public async Task<bool> RevertGeneratingJobsToPending()
        {
            TableQuery<JobInfoEntity> tableQuery = new TableQuery<JobInfoEntity>().Where(TableQuery.GenerateFilterConditionForInt("Status", QueryComparisons.Equal, (int)RepositoryStatus.JOB_STATUS.Generating));
            IEnumerable<JobInfoEntity> data = _jobInfoTable.ExecuteQuery<JobInfoEntity>(tableQuery);
            List<JobInfoEntity> entities = new List<JobInfoEntity>();
            foreach (var item in data)
            {
                // revert status to pending and add to list to be updated
                item.Status = (int)RepositoryStatus.JOB_STATUS.Pending;
                entities.Add(item);
            }

            if (entities.Count == 0)
                return true;

            List<TableBatchOperation> batchOps = new List<TableBatchOperation>();
            TableBatchOperation currentOp = new TableBatchOperation();

            int count = 0;
            foreach(var entity in entities)
            {
                if(count == 50)
                {
                    batchOps.Add(currentOp);
                    currentOp = new TableBatchOperation();
                    count = 0;
                }

                currentOp.Add(TableOperation.Replace(entity));
                count++;
            }

            if (count > 0)
                batchOps.Add(currentOp);


            List<Task<IList<TableResult>>> tasks = new List<Task<IList<TableResult>>>();
            foreach (var op in batchOps)
            {
                tasks.Add(_jobInfoTable.ExecuteBatchAsync(op));
            }

            IList<TableResult>[] results = await Task.WhenAll(tasks);
            bool success = results.All(tbr => tbr.All(tr => tr.HttpStatusCode == 204));

            if (!success)
                Log.Error("Failed to revert all generating jobs to pending");

            return success;
        }

        public async Task<int> DeleteOldRequests(DateTime cutoff)
        {
            TableQuery<JobInfoEntity> tableQuery = new TableQuery<JobInfoEntity>().Where(TableQuery.GenerateFilterConditionForDate("CreationDate", QueryComparisons.LessThanOrEqual, cutoff));
            IEnumerable<JobInfoEntity> data = _jobInfoTable.ExecuteQuery<JobInfoEntity>(tableQuery);

            int count = 0;
            foreach (var item in data)
            {
                bool success = await DeleteRequest(item.JobId);
                if (success)
                    count++;
            }

            return count;
        }

        public JobInfoEntity GetRequestInfo(Guid requestId)
        {
            TableQuery<JobInfoEntity> tableQuery = new TableQuery<JobInfoEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, requestId.ToString()));

            IEnumerable<JobInfoEntity> data = _jobInfoTable.ExecuteQuery<JobInfoEntity>(tableQuery);
            List<JobInfoEntity> entities = new List<JobInfoEntity>();
            foreach (var item in data)
                entities.Add(item);

            if (entities.Count == 0)
                return null;

            return entities.FirstOrDefault();
        }

        public async Task<Document> GetGeneratedReport(Guid requestId)
        {
            // get generated report from blob storage
            return await GetEntityFromBlob<Document>(requestId, _documentContainer);
        }

        public async Task<ServiceError> GetError(Guid requestId)
        {
            // get error from blob storage
            return await GetEntityFromBlob<ServiceError>(requestId, _documentContainer);
        }

        public async Task<Metrics> GetMetrics(Guid requestId)
        {
            // get metrics from blob storage
            return await GetEntityFromBlob<Metrics>(requestId, _documentContainer);
        }

        public async Task<TagTree> GetTagTree(Guid requestId)
        {
            // get tag tree from blob storage
            return await GetEntityFromBlob<TagTree>(requestId, _documentContainer);
        }

        public async Task<JobRequestData> GetOldestPendingJobAndGenerate()
        {
            try
            {
                Log.Info($"[AzureStorageManager] In GetOldestPendingJobAndGenerate()");
                TableQuery<JobInfoEntity> tableQuery = new TableQuery<JobInfoEntity>().Where(TableQuery.GenerateFilterConditionForInt("Status", QueryComparisons.Equal, (int)RepositoryStatus.JOB_STATUS.Pending));

                List<JobInfoEntity> entities;
                JobInfoEntity oldestEntity = null;

                bool fourTwelveEx = true;
                TableResult result = null;
                while(fourTwelveEx)
                {
                    try
                    { 
                        entities = _jobInfoTable.ExecuteQuery<JobInfoEntity>(tableQuery).ToList();

                        Log.Info($"[AzureStorageManager] Number of entities returned: {entities.Count}");
                        if (entities.Count == 0)
                            return null;

                        oldestEntity = entities.OrderBy(d => d.CreationDate).ToArray().FirstOrDefault();
                        Log.Info($"[AzureStorageManager] Oldest entity retrieved: {oldestEntity.JobId}");

                        // Set this entity to locked so no others use it and set to generating
                        oldestEntity.Status = (int)RepositoryStatus.JOB_STATUS.Generating;
                        var op = TableOperation.Replace(oldestEntity);
                        result = await _jobInfoTable.ExecuteAsync(op);
                        fourTwelveEx = false;
                    } 
                    catch(StorageException ex)
                    {
                        if(ex.RequestInformation.HttpStatusCode == 412)
                        {
                            Log.Warn("[AzureStorageManager] Entity has changed since it was retrieved. Trying again");
                            continue;
                        } 
                        else
                        {
                            Log.Error($"[AzureSTorageManager] StorageException in GetOldestPendingJobAndGenerate: {ex.Message}");
                            return null;
                        }
                    }
                }

                bool success = result.HttpStatusCode == 204;

                if (!success)
                {
                    Log.Error($"[AzureStorageManger] Failed to update job entity [{oldestEntity.JobId}] to generating.");
                    return null;
                }

                Log.Info($"[AzureStorageManger] Updated job entity [{oldestEntity.JobId}] to generating.");

                // Get the template for this job
                Template template = await GetEntityFromBlob<Template>(oldestEntity.JobId, _templateContainer);

                Log.Info($"[AzureStorageManager] Got the template from blob storage");

                return new JobRequestData
                {
                    Template = template,
                    RequestType = (RepositoryStatus.REQUEST_TYPE)oldestEntity.Type
                };
            } catch(Exception e)
            {
                Log.Error($"[AzureSTorageManager] Exception in GetOldestPendingJobAndGenerate: {e.Message}");
                return null;
            }
        }

        private async Task<T> GetEntityFromBlob<T>(Guid id, string container)
        {
            CloudBlockBlob blob = GetBlobContainerFromContainerName(container).GetBlockBlobReference(id.ToString());

             if (!await blob.ExistsAsync())
            {
                Log.Error($"Blob [{id}] does not exist in container {container}");
                return default(T);
            }
            
            MemoryStream memoryStream = new MemoryStream();
            await blob.DownloadToStreamAsync(memoryStream);
            string jsonBack = Encoding.UTF8.GetString(memoryStream.ToArray());
            var ret = JsonConvert.DeserializeObject<T>(jsonBack);
            return ret;
        }

        private CloudBlobContainer GetBlobContainerFromContainerName(string containerName)
        {
            var blob = containerName.Equals(_templateContainer) ? _jobTemplateBlob : _jobDocumentBlob;
            return blob;
        }

        private async Task UploadBlob<T>(T target, string id, string containerName)
        {
            string data = JsonConvert.SerializeObject(target);
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
            byte[] byteData = Convert.FromBase64String(base64);
            try
            {
                CloudBlockBlob blob = GetBlobContainerFromContainerName(containerName).GetBlockBlobReference(id);

                await blob.UploadFromByteArrayAsync(byteData, 0, byteData.Length);
            }
            catch (Exception e)
            {
                Log.Error($"Exception uploading blob: {e}");
            }
        }

        private async Task DeleteBlob(Guid id, string containerName)
        {
            CloudBlockBlob blob = GetBlobContainerFromContainerName(containerName).GetBlockBlobReference(id.ToString());

            if (!await blob.ExistsAsync())
            {
                Log.Error($"Blob [{id}] does not exist in container {containerName}");
                return;
            }

            await blob.DeleteAsync();
        }

        public async Task<DocumentPerformance> GetDocumentPerformance(Guid requestId)
        {
            return await GetEntityFromBlob<DocumentPerformance>(requestId, _docPerformanceContainer);
        }

        public async Task PostDocumentPerformance(DocumentPerformance data, string guid)
        {
            data.guid = guid;
            await UploadBlob(data, guid, _docPerformanceContainer);
        }

        public async Task PostCachedTemplate(CachedTemplate cachedTemplate)
        {
            await UploadBlob(cachedTemplate, cachedTemplate.TemplateID, _templateContainer);
        }

        public async Task<CachedTemplate> GetCachedTemplate(Guid guid)
        {
            return await GetEntityFromBlob<CachedTemplate>(guid, _templateContainer);
        }

        public async Task DeleteCachedTemplate(Guid guid)
        {
            await DeleteBlob(guid, _templateContainer);
        }
    }
}
