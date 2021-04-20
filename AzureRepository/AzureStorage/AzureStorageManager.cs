using System;
using System.Threading.Tasks;
using AzureRepositoryPlugin.AzureStorage;
using Microsoft.Azure.Cosmos.Table;
using RESTfulEngine.DocumentRepository;
using AzureStorage.Blobs;
using AzureStorage.Tables;
using RESTfulEngine.Models;
using log4net;
using System.Configuration;
using Azure.Storage.Blobs.Models;
using Azure;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Linq;

namespace AzureRepositoryPlugin
{
    public class AzureStorageManager
    {
        private readonly ICloudTableFactory _tableFactory;
        private readonly IBlobContainerFactory _blobFactory;

        private readonly string _storageConnectionString;

        protected const string JOB_INFO_TABLE_NAME = "RestJobInfoTable";
        private const string JOB_BLOB_NAME = "restjobblobs";

        private const string TEMPLATE_CONTAINER = "templates";
        private const string DOCUMENT_CONTAINER = "generateddocuments";

        protected ICloudTableWrapper<JobInfoEntity> _jobInfoTable;

        protected IBlobContainerWrapper _jobBlobContainer;

        private readonly string PartitionKey = Guid.Empty.ToString();

        private static readonly ILog Log = LogManager.GetLogger(typeof(AzureStorageManager));

        public AzureStorageManager(ICloudTableFactory tableFactory, IBlobContainerFactory blobFactory)
        {
            _tableFactory = tableFactory;
            _blobFactory = blobFactory;

            _storageConnectionString = ConfigurationManager.AppSettings["AzureRepository:StorageConnectionString"];
        }

        public async Task Init()
        {
            _jobInfoTable = await _tableFactory.CreateCloudTableWrapper<JobInfoEntity>(_storageConnectionString, JOB_INFO_TABLE_NAME);
            _jobBlobContainer = await _blobFactory.CreateBlobContainerWrapper(_storageConnectionString, JOB_BLOB_NAME);
        }

        public async Task<bool> AddRequest(JobRequestData request)
        {
            // Add a request to storage
            JobInfoEntity entity = JobInfoEntity.FromJobRequestData(request, PartitionKey);
            entity.Status = RepositoryStatus.JOB_STATUS.Pending;

            TableResult result = await _jobInfoTable.UpsertEntity(entity);
            bool success = result.HttpStatusCode == 204;

            if (success)
                Log.Debug($"Added request [{request.Template.Guid}] to table storage");
            else
                Log.Error($"Failed to add request [{request.Template.Guid}] to table storage: {result.HttpStatusCode}");

            string templateData = JsonConvert.SerializeObject(request.Template);
            string templateBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(templateData));

            Response<BlobContentInfo> response = null;
            try
            {
                response = await _jobBlobContainer.UploadBase64(DOCUMENT_CONTAINER, entity.JobId.ToString(), templateBase64);
            } catch(Exception e)
            {
                Log.Error($"Exception {e}");
            }

            success &= response.GetRawResponse().Status == 201;

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

            entity.Status = newStatus;

            var result = await _jobInfoTable.ReplaceEntity(entity);
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
            entity.Status = RepositoryStatus.JOB_STATUS.Complete;

            var result = await _jobInfoTable.ReplaceEntity(entity);
            bool success = result.HttpStatusCode == 204;

            if (success)
                Log.Debug($"Updated request [{requestId}] status to Complete");
            else
                Log.Error($"Failed to update request [{requestId}] status to Complete: {result.HttpStatusCode}");


            // Upload generated document to blob
            Response<BlobContentInfo> response = null;

            // May be able to do this on generic object and won't need switch
            switch (generatedEntity)
            {
                case Document document:
                    string documentData = JsonConvert.SerializeObject(document);
                    string documentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(documentData));
                    response = await _jobBlobContainer.UploadBase64(DOCUMENT_CONTAINER, requestId.ToString(), documentBase64);
                    //response = await _jobBlobContainer.UploadBinaryData(CONTAINER_NAME, requestId.ToString(), document.Data);
                    break;
                case Metrics metrics:
                    string metricsData = JsonConvert.SerializeObject(metrics);
                    string metricsBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(metricsData));
                    response = await _jobBlobContainer.UploadBase64(DOCUMENT_CONTAINER, requestId.ToString(), metricsBase64);
                    break;
                case TagTree tagTree:
                    string tagTreeData = JsonConvert.SerializeObject(tagTree);
                    string tagTreeBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(tagTreeData));
                    response = await _jobBlobContainer.UploadBase64(DOCUMENT_CONTAINER, requestId.ToString(), tagTreeBase64);
                    break;
                case ServiceError error:
                    string errorData = JsonConvert.SerializeObject(error);
                    string errorBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(errorData));
                    response = await _jobBlobContainer.UploadBase64(DOCUMENT_CONTAINER, requestId.ToString(), errorBase64);
                    break;
                default:
                    Log.Error("Unable to store blob for unknown entity type");
                    return false;
            }

            success &= response.GetRawResponse().Status == 201;

            if (success)
                Log.Debug($"Added generated entity [{requestId}] status to blob storage");
            else
                Log.Error($"Failed to add generated entity [{requestId}] to blob storage: {response.GetRawResponse().Status}");

            return success;
        }

        public async Task<bool> DeleteRequest(Guid requestId)
        {
            // Remove request from storage
            JobInfoEntity entity = await GetRequestInfo(requestId);
            TableResult result = await _jobInfoTable.DeleteEntity(entity);
            bool success = result.HttpStatusCode == 204;

            if (success)
                Log.Debug($"Successfully deleted request [{requestId}] from table storage");
            else
                Log.Error($"Failed to delete request [{requestId}] from table storage: {result.HttpStatusCode}");

            // Delete template if exists
            if (await _jobBlobContainer.DoesBlobExist(TEMPLATE_CONTAINER, requestId.ToString()))
            {
                var response = await _jobBlobContainer.DeleteBlob(TEMPLATE_CONTAINER, requestId.ToString());
                success &= response.GetRawResponse().Status == 202;

                if (success)
                    Log.Debug($"Successfully deleted request [{requestId}] from blob storage");
                else
                    Log.Error($"Failed to delete request [{requestId}] from blob storage: {result.HttpStatusCode}");
            }

            // Delete generated doc if exists
            if (await _jobBlobContainer.DoesBlobExist(DOCUMENT_CONTAINER, requestId.ToString()))
            {
                var response = await _jobBlobContainer.DeleteBlob(DOCUMENT_CONTAINER, requestId.ToString());
                success &= response.GetRawResponse().Status == 202;

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
            string filter = CloudTableUtils.Equal(CloudTableUtils.ROW_KEY, requestId.ToString());
            JobInfoEntity entity = await _jobInfoTable.GetSingleEntity(filter);
            return entity;
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
            string filter = CloudTableUtils.And(CloudTableUtils.EqualInt("Status", (int)RepositoryStatus.JOB_STATUS.Pending), CloudTableUtils.EqualBool("Locked", false));
            JobInfoEntity[] entities = await _jobInfoTable.GetEntities(filter);
            if (entities.Length == 0)
                return null;

            JobInfoEntity oldestEntity = entities.OrderBy(d => d.CreationDate).ToArray().FirstOrDefault();

            // Set this entity to locked so no others use it and set to generating
            oldestEntity.Status = RepositoryStatus.JOB_STATUS.Generating;
            var result = await _jobInfoTable.ReplaceEntity(oldestEntity);
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
                RequestType = oldestEntity.Type
            };
        }

        private async Task<T> GetEntityFromBlob<T>(Guid id, string container)
        {
            if (!await _jobBlobContainer.DoesBlobExist(TEMPLATE_CONTAINER, id.ToString())) {
                Log.Error($"Blob [{id}] does not exist in container {container}");
                return default(T);
            }

            MemoryStream ms = await _jobBlobContainer.GetInMemory(DOCUMENT_CONTAINER, id.ToString());
            string base64 = System.Text.Encoding.ASCII.GetString(ms.ToArray());
            T ret = JsonConvert.DeserializeObject<T>(base64);
            return ret;
        }
    }
}
