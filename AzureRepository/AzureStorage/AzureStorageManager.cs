using System;
using System.Threading.Tasks;
using AzureRepositoryPlugin.AzureStorage;
using RESTfulEngine.DocumentRepository;
using RESTfulEngine.Models;
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

namespace AzureRepositoryPlugin
{
    public class AzureStorageManager
    {
        private readonly string _storageConnectionString;

        protected const string JOB_INFO_TABLE_NAME = "RestJobInfoTable";
        private const string JOB_BLOB_NAME = "restjobblobs";

        private const string TEMPLATE_CONTAINER = "templates";
        private const string DOCUMENT_CONTAINER = "generateddocuments";

        //protected ICloudTableWrapper<JobInfoEntity> _jobInfoTable;
        private CloudTable _jobInfoTable;
        private CloudStorageAccount _storageAccount;
        private CloudTableClient _tableClient;

        private CloudBlobClient _blobClient;
        private CloudBlobContainer _jobTemplateBlob;
        private CloudBlobContainer _jobDocumentBlob;

        private readonly string PartitionKey = Guid.Empty.ToString();

        private static readonly ILog Log = LogManager.GetLogger(typeof(AzureStorageManager));

        public AzureStorageManager()
        {
            _storageConnectionString = ConfigurationManager.AppSettings["AzureRepository:StorageConnectionString"];
        }

        public void Init()
        {
            try
            {
                _storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
                _tableClient = _storageAccount.CreateCloudTableClient();

                _jobInfoTable = _tableClient.GetTableReference(JOB_INFO_TABLE_NAME);
                _jobInfoTable.CreateIfNotExists();

                _blobClient = _storageAccount.CreateCloudBlobClient();
                _jobTemplateBlob = _blobClient.GetContainerReference(TEMPLATE_CONTAINER);
                _jobTemplateBlob.CreateIfNotExists();

                _jobDocumentBlob = _blobClient.GetContainerReference(DOCUMENT_CONTAINER);
                _jobDocumentBlob.CreateIfNotExists();
            } catch(StorageException e)
            {
                Log.Error("STORAGE EXCEPTION: " + e);
            } catch (Exception ex)
            {
                Log.Error("EXCEPTION: " + ex);
            }
            //_jobInfoTable = await _tableFactory.CreateCloudTableWrapper<JobInfoEntity>(_storageConnectionString, JOB_INFO_TABLE_NAME);
            //_jobBlobContainer = await _blobFactory.CreateBlobContainerWrapper(_storageConnectionString, JOB_BLOB_NAME);
        }

        public async Task<bool> AddRequest(JobRequestData request)
        {
            // Add a request to storage
            JobInfoEntity entity = JobInfoEntity.FromJobRequestData(request, PartitionKey);
            entity.Status = (int)RepositoryStatus.JOB_STATUS.Pending;

            var op = TableOperation.Insert(entity);
            TableResult result = await _jobInfoTable.ExecuteAsync(op);
            bool success = result.HttpStatusCode == 204;

            if (success)
                Log.Debug($"Added request [{request.Template.Guid}] to table storage");
            else
                Log.Error($"Failed to add request [{request.Template.Guid}] to table storage: {result.HttpStatusCode}");


            // Upload template to blob storage
            await UploadBlob<Template>(request.Template, entity.JobId.ToString(), TEMPLATE_CONTAINER);

            if (success)
                Log.Debug($"Added template [{request.Template.Guid}] to blob storage");
            else
                Log.Error($"Failed to add template [{request.Template.Guid}] to blob storage: {result.HttpStatusCode}");

            return success;
        }

        public async Task<bool> UpdateRequest(Guid requestId, RepositoryStatus.JOB_STATUS newStatus)
        {
            // Update status for specific request
            JobInfoEntity entity = await GetRequestInfo(requestId);
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
            JobInfoEntity entity = await GetRequestInfo(requestId);
            entity.Status = (int)RepositoryStatus.JOB_STATUS.Complete;

            var op = TableOperation.Replace(entity);
            TableResult result = await _jobInfoTable.ExecuteAsync(op);
            bool success = result.HttpStatusCode == 204;

            if (success)
                Log.Debug($"Updated request [{requestId}] status to Complete");
            else
                Log.Error($"Failed to update request [{requestId}] status to Complete: {result.HttpStatusCode}");


            // Upload generated document to blob
            await UploadBlob<T>(generatedEntity, entity.JobId.ToString(), DOCUMENT_CONTAINER);

            if (success)
                Log.Debug($"Added generated entity [{requestId}] status to blob storage");
            else
                Log.Error($"Failed to add generated entity [{requestId}] to blob storage");

            return success;
        }

        public async Task<bool> DeleteRequest(Guid requestId)
        {
            // Remove request from storage
            JobInfoEntity entity = await GetRequestInfo(requestId);
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

        public async Task<JobInfoEntity> GetRequestInfo(Guid requestId)
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
            return await GetEntityFromBlob<Document>(requestId, DOCUMENT_CONTAINER);
        }

        public async Task<ServiceError> GetError(Guid requestId)
        {
            // get error from blob storage
            return await GetEntityFromBlob<ServiceError>(requestId, DOCUMENT_CONTAINER);
        }

        public async Task<Metrics> GetMetrics(Guid requestId)
        {
            // get metrics from blob storage
            return await GetEntityFromBlob<Metrics>(requestId, DOCUMENT_CONTAINER);
        }

        public async Task<TagTree> GetTagTree(Guid requestId)
        {
            // get tag tree from blob storage
            return await GetEntityFromBlob<TagTree>(requestId, DOCUMENT_CONTAINER);
        }

        public async Task<JobRequestData> GetOldestPendingJobAndGenerate()
        {
            TableQuery<JobInfoEntity> tableQuery = new TableQuery<JobInfoEntity>();
            tableQuery = new TableQuery<JobInfoEntity>().Where(TableQuery.GenerateFilterConditionForInt("Status", QueryComparisons.Equal, (int)RepositoryStatus.JOB_STATUS.Pending));

            IEnumerable<JobInfoEntity> data = _jobInfoTable.ExecuteQuery<JobInfoEntity>(tableQuery);
            List<JobInfoEntity> entities = new List<JobInfoEntity>();
            foreach (var item in data)
                entities.Add(item);

            if (entities.Count == 0)
                return null;

            JobInfoEntity oldestEntity = entities.OrderBy(d => d.CreationDate).ToArray().FirstOrDefault();

            // Set this entity to locked so no others use it and set to generating
            oldestEntity.Status = (int)RepositoryStatus.JOB_STATUS.Generating;
            var op = TableOperation.Replace(oldestEntity);
            TableResult result = await _jobInfoTable.ExecuteAsync(op);
            bool success = result.HttpStatusCode == 204;

            if (!success)
            {
                Log.Error($"Failed to update job entity [{oldestEntity.JobId}] to generating.");
                return null;
            }

            Log.Debug($"Updated job entity [{oldestEntity.JobId}] to generating.");

            // Get the template for this job
            Template template = await GetEntityFromBlob<Template>(oldestEntity.JobId, TEMPLATE_CONTAINER);

            return new JobRequestData
            {
                Template = template,
                RequestType = (RepositoryStatus.REQUEST_TYPE)oldestEntity.Type
            };
        }

        private async Task<T> GetEntityFromBlob<T>(Guid id, string container)
        {
            CloudBlockBlob blob = null;
            if(container.Equals(TEMPLATE_CONTAINER))
            {
                blob = _jobTemplateBlob.GetBlockBlobReference(id.ToString());
            } 
            else
            {
                blob = _jobDocumentBlob.GetBlockBlobReference(id.ToString());
            }

            if (!await blob.ExistsAsync())
            {
                Log.Error($"Blob [{id}] does not exist in container {container}");
                return default(T);
            }

            MemoryStream memoryStream = new MemoryStream();
            await blob.DownloadToStreamAsync(memoryStream);
            string jsonBack = Encoding.UTF8.GetString(memoryStream.ToArray());
            T ret = JsonConvert.DeserializeObject<T>(jsonBack);
            return ret;
        }

        private async Task UploadBlob<T>(T target, string id, string containerName)
        {
            string data = JsonConvert.SerializeObject(target);
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
            byte[] byteData = Convert.FromBase64String(base64);
            try
            {
                CloudBlockBlob blob = null;
                if(containerName.Equals(TEMPLATE_CONTAINER))
                    blob = _jobTemplateBlob.GetBlockBlobReference(id);
                else
                    blob = _jobDocumentBlob.GetBlockBlobReference(id);

                await blob.UploadFromByteArrayAsync(byteData, 0, data.Length);
            }
            catch (Exception e)
            {
                Log.Error($"Exception uploading blob: {e}");
            }
        }
    }
}
