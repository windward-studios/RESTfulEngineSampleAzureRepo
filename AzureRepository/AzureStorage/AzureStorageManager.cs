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

            //TableResult result = await _jobInfoTable.UpsertEntity(entity);
            var insertOp = TableOperation.Insert(entity);
            TableResult result = await _jobInfoTable.ExecuteAsync(insertOp);
            bool success = result.HttpStatusCode == 204;

            if (success)
                Log.Debug($"Added request [{request.Template.Guid}] to table storage");
            else
                Log.Error($"Failed to add request [{request.Template.Guid}] to table storage: {result.HttpStatusCode}");

            string templateData = JsonConvert.SerializeObject(request.Template);
            string templateBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(templateData));
            byte[] data = Convert.FromBase64String(templateBase64);

            try
            {
                CloudBlockBlob blob = _jobTemplateBlob.GetBlockBlobReference(entity.JobId.ToString());
                await blob.UploadFromByteArrayAsync(data, 0, data.Length);
            } catch(Exception e)
            {
                Log.Error($"Exception uploading blob: {e}");
            }

            //Response<BlobContentInfo> response = null;
            //try
            //{
            //    response = await _jobBlobContainer.UploadBase64(TEMPLATE_CONTAINER, entity.JobId.ToString(), templateBase64);
            //} catch(Exception e)
            //{
            //    Log.Error($"Exception {e}");
            //}

            //success &= response.GetRawResponse().Status == 201;

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

            //var result = await _jobInfoTable.ReplaceEntity(entity);
            //bool success = result.HttpStatusCode == 204;
            var insertOp = TableOperation.Replace(entity);
            TableResult result = await _jobInfoTable.ExecuteAsync(insertOp);
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

            //var result = await _jobInfoTable.ReplaceEntity(entity);
            //bool success = result.HttpStatusCode == 204;
            var op = TableOperation.Replace(entity);
            TableResult result = await _jobInfoTable.ExecuteAsync(op);
            bool success = result.HttpStatusCode == 204;

            if (success)
                Log.Debug($"Updated request [{requestId}] status to Complete");
            else
                Log.Error($"Failed to update request [{requestId}] status to Complete: {result.HttpStatusCode}");


            // Upload generated document to blob
            //Response<BlobContentInfo> response = null;
            CloudBlockBlob blob = null;
            // May be able to do this on generic object and won't need switch
            switch (generatedEntity)
            {
                case Document document:
                    string documentData = JsonConvert.SerializeObject(document);
                    string documentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(documentData));
                    //response = await _jobBlobContainer.UploadBase64(DOCUMENT_CONTAINER, requestId.ToString(), documentBase64);
                    blob = _jobDocumentBlob.GetBlockBlobReference(entity.JobId.ToString());
                    await blob.UploadTextAsync(documentBase64);
                    //response = await _jobBlobContainer.UploadBinaryData(CONTAINER_NAME, requestId.ToString(), document.Data);
                    break;
                case Metrics metrics:
                    string metricsData = JsonConvert.SerializeObject(metrics);
                    string metricsBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(metricsData));
                    //response = await _jobBlobContainer.UploadBase64(DOCUMENT_CONTAINER, requestId.ToString(), metricsBase64);
                    blob = _jobDocumentBlob.GetBlockBlobReference(entity.JobId.ToString());
                    await blob.UploadTextAsync(metricsBase64);
                    break;
                case TagTree tagTree:
                    string tagTreeData = JsonConvert.SerializeObject(tagTree);
                    string tagTreeBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(tagTreeData));
                    //response = await _jobBlobContainer.UploadBase64(DOCUMENT_CONTAINER, requestId.ToString(), tagTreeBase64);
                    blob = _jobDocumentBlob.GetBlockBlobReference(entity.JobId.ToString());
                    await blob.UploadTextAsync(tagTreeBase64);
                    break;
                case ServiceError error:
                    string errorData = JsonConvert.SerializeObject(error);
                    string errorBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(errorData));
                    //response = await _jobBlobContainer.UploadBase64(DOCUMENT_CONTAINER, requestId.ToString(), errorBase64);
                    blob = _jobDocumentBlob.GetBlockBlobReference(entity.JobId.ToString());
                    await blob.UploadTextAsync(errorBase64);
                    break;
                default:
                    Log.Error("Unable to store blob for unknown entity type");
                    return false;
            }

            //success &= response.GetRawResponse().Status == 201;

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
            //TableResult result = await _jobInfoTable.DeleteEntity(entity);
            //bool success = result.HttpStatusCode == 204;

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

        public async Task<JobInfoEntity> GetRequestInfo(Guid requestId)
        {
            // Get a single JobInfoEntity
            //string filter = CloudTableUtils.Equal(CloudTableUtils.ROW_KEY, requestId.ToString());
            //JobInfoEntity entity = await _jobInfoTable.GetSingleEntity(filter);

            //var op = TableOperation.Retrieve(PartitionKey, requestId.ToString());
            //TableResult result = await _jobInfoTable.ExecuteAsync(op);
            //JobInfoEntity e = result.Result as JobInfoEntity;
            //return e;

            TableQuery<JobInfoEntity> tableQuery = new TableQuery<JobInfoEntity>();
            tableQuery = new TableQuery<JobInfoEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, requestId.ToString()));

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
            // get the oldest pending job, set the lock property to true (atomic), and set status to generating
            //string filter = CloudTableUtils.And(CloudTableUtils.EqualInt("Status", (int)RepositoryStatus.JOB_STATUS.Pending), CloudTableUtils.EqualBool("Locked", false));
            //JobInfoEntity[] entities = await _jobInfoTable.GetEntities(filter);
            //if (entities.Length == 0)
            //    return null;
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
            //string base64 = System.Text.Encoding.ASCII.GetString(memoryStream.ToArray());
            //string base64 = Convert.ToBase64String(memoryStream.ToArray());
            string jsonBack = Encoding.UTF8.GetString(memoryStream.ToArray());
            T ret = JsonConvert.DeserializeObject<T>(jsonBack);
            return ret;

            //if (!await _jobBlobContainer.DoesBlobExist(DOCUMENT_CONTAINER, id.ToString())) {
            //    Log.Error($"Blob [{id}] does not exist in container {container}");
            //    return default(T);
            //}

            //MemoryStream ms = await _jobBlobContainer.GetInMemory(DOCUMENT_CONTAINER, id.ToString());
            //string base64 = System.Text.Encoding.ASCII.GetString(ms.ToArray());
            //T ret = JsonConvert.DeserializeObject<T>(base64);
            //return ret;
        }
    }
}
