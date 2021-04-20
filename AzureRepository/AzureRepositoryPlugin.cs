using AzureRepositoryPlugin;
using AzureRepositoryPlugin.AzureStorage;
using AzureRepositoryPlugin.EventBus;
using log4net;
using Newtonsoft.Json;
using RESTfulEngine.BusinessLogic;
using RESTfulEngine.DocumentRepository;
using RESTfulEngine.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace AzureRepository
{
    public class AzureRepositoryPlugin : IRepository
    {

        /// <summary>Delete any jobs older than this.</summary>
        private readonly TimeSpan timeSpanDeleteOldJobs;

        /// <summary>How often we check for old jobs.</summary>
        private readonly TimeSpan timeSpanCheckOldJobs;

        private DateTime datetimeLastCheckOldJobs;

        private IJobHandler JobHandler { get; set; }

        private static readonly ILog Log = LogManager.GetLogger(typeof(AzureRepositoryPlugin));

        private bool shutDown;

        private EventBusProducer _producer;

        private StorageManager StorageManager;

        public AzureRepositoryPlugin()
        {
            // Config value setup
            string num = ConfigurationManager.AppSettings["hours.delete.jobs"] ?? "";
            if (!int.TryParse(num, out int hours))
                hours = 24;
            else
                hours = Math.Max(1, hours);

            timeSpanDeleteOldJobs = TimeSpan.FromHours(hours);

            // check every 24th of timeSpanDeleteOldJobs. So if delete 24+ hours old, check every hour
            timeSpanCheckOldJobs = TimeSpan.FromMinutes(timeSpanDeleteOldJobs.TotalMinutes / 24);

            // we give it the timespan until the first check - to first get anything it grabs at startup.
            datetimeLastCheckOldJobs = DateTime.Now;

            shutDown = false;

            // Inititalize storage
            StorageManager = new StorageManager();

            // Initialize Service Bus components
            _producer = new EventBusProducer();

            AddConsumers();

            // Start the processor
            _producer.Start();
        }

        private void AddConsumers()
        {
            EventConsumer.OnMessageConsumed += MessageHandler;
        }

        private async Task MessageHandler(object sender, EventArgs<JobRequestData> args)
        {
            // Add the request to storage
            JobRequestData jobData = args.Message;
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            bool success = await storage.AddRequest(jobData);

            if (!success)
            {
                Log.Error($"Failed to add job request [{jobData.Template.Guid}] to storage");
            }

            Log.Debug($"Added job request [{jobData.Template.Guid}] to storage");
        }

        public string CreateRequest(Template template, RepositoryStatus.REQUEST_TYPE requestType)
        {
            template.Guid = Guid.NewGuid().ToString();

            JobRequestData data = new JobRequestData
            {
                Template = template,
                RequestType = requestType,
                Action = JobRequestAction.CREATE,   //idk if we need this property at all
                CreationDate = DateTime.UtcNow
            };

            _producer.Publish(data);

            Log.Debug($"Created Request of type {requestType} for template {template.Guid}. Request added to queue.");

            return template.Guid;
        }

        public RepositoryRequest TakeRequest()
        {
            var task = Task.Run(async () => await TakeRequestAsync());
            task.Wait();
            RepositoryRequest ret = task.Result;
            return ret;
        }

        private async Task<RepositoryRequest> TakeRequestAsync()
        {
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            JobRequestData job = await storage.GetOldestPendingJobAndGenerate();

            return new RepositoryRequest(job.Template, job.RequestType);
        }

        public void DeleteReport(string guid)
        {
            var task = Task.Run(async () => await DeleteReportAsync(guid));
            task.Wait();
        }

        private async Task<bool> DeleteReportAsync(string guid)
        {
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            bool result = await storage.DeleteRequest(Guid.Parse(guid));
            return result;
        }

        public ServiceError GetError(string guid)
        {
            var task = Task.Run<ServiceError>(async () => await GetErrorAsync(guid));
            task.Wait();
            ServiceError res = task.Result;
            return res;
        }

        private async Task<ServiceError> GetErrorAsync(string guid)
        {
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            var result = await storage.GetError(Guid.Parse(guid));
            return result;
        }

        public Metrics GetMetrics(string guid)
        {
            var task = Task.Run<Metrics>(async () => await GetMetricsAsync(guid));
            task.Wait();
            Metrics res = task.Result;
            return res;
        }

        private async Task<Metrics> GetMetricsAsync(string guid)
        {
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            var result = await storage.GetMetrics(Guid.Parse(guid));
            return result;
        }

        public Document GetReport(string guid)
        {
            var task = Task.Run<Document>(async () => await GetReportAsync(guid));
            task.Wait();
            Document res = task.Result;
            return res;
        }

        private async Task<Document> GetReportAsync(string guid)
        {
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            var result = await storage.GetGeneratedReport(Guid.Parse(guid));
            return result;
        }

        public RequestStatus GetReportStatus(string guid)
        {
            var task = Task.Run<RequestStatus>(async () => await GetReportStatusAsync(guid));
            task.Wait();
            RequestStatus res = task.Result;
            return res;
        }

        private async Task<RequestStatus> GetReportStatusAsync(string guid)
        {
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            JobInfoEntity result = await storage.GetRequestInfo(Guid.Parse(guid));
            return new RequestStatus(result.Status, result.Type);
        }

        public TagTree GetTagTree(string guid)
        {
            var task = Task.Run<TagTree>(async () => await GetTagTreeAsync(guid));
            task.Wait();
            TagTree res = task.Result;
            return res;
        }

        private async Task<TagTree> GetTagTreeAsync(string guid)
        {
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            var result = await storage.GetTagTree(Guid.Parse(guid));
            return result;
        }

        public void SaveError(Template template, ServiceError error)
        {
            var task = Task.Run(async () => await SaveErrorAsync(template, error));
            task.Wait();
            CompleteJob(template);
        }

        private async Task SaveErrorAsync(Template template, ServiceError error)
        {
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            var result = await storage.CompleteRequest(Guid.Parse(template.Guid), error);
            if (result)
                Log.Debug($"Successfully saved error {template.Guid}");
            else
                Log.Error($"Failed to save error {template.Guid}");
        }

        public void SaveMetrics(Template template, Metrics metrics)
        {
            var task = Task.Run(async () => await SaveMetricsAsync(template, metrics));
            task.Wait();
            CompleteJob(template);
        }

        private async Task SaveMetricsAsync(Template template, Metrics metrics)
        {
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            var result = await storage.CompleteRequest(Guid.Parse(template.Guid), metrics);
            if (result)
                Log.Debug($"Successfully saved metrics {template.Guid}");
            else
                Log.Error($"Failed to save metrics {template.Guid}");
        }

        public void SaveReport(Template template, Document document)
        {
            var task = Task.Run(async () => await SaveReportAsync(template, document));
            task.Wait();
            CompleteJob(template);
        }

        private async Task SaveReportAsync(Template template, Document document)
        {
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            var result = await storage.CompleteRequest(Guid.Parse(template.Guid), document);
            if (result)
                Log.Debug($"Successfully saved document {template.Guid}");
            else
                Log.Error($"Failed to save document {template.Guid}");
        }

        public void SaveTagTree(Template template, TagTree tree)
        {
            var task = Task.Run(async () => await SaveTagTreeAsync(template, tree));
            task.Wait();
            CompleteJob(template);
        }

        private async Task SaveTagTreeAsync(Template template, TagTree tree)
        {
            AzureStorageManager storage = await StorageManager.GetAzureStorageManager();
            var result = await storage.CompleteRequest(Guid.Parse(template.Guid), tree);
            if (result)
                Log.Debug($"Successfully saved tag tree {template.Guid}");
            else
                Log.Error($"Failed to save tag tree {template.Guid}");
        }

        public void SetJobHandler(IJobHandler handler)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"SetJobHandler({handler})");
            JobHandler = handler;
        }

        public void ShutDown()
        {
            shutDown = true;

            _producer.Stop();

            // Need to set all generating requests in azure storage back to pending
            // Also need those reverted generating jobs to generate once started back up (maybe an additional column?)
        }

        private void CompleteJob(Template template)
        {
            if (shutDown || string.IsNullOrEmpty(template.Callback))
                return;

            string url = template.Callback.Replace("{guid}", template.Guid);
            try
            {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = client.PostAsync(url, null).Result)
                    if (response.StatusCode != HttpStatusCode.OK && Log.IsInfoEnabled)
                        Log.Info($"Callback to {url} returned status code {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Log.Warn($"Callback for job {template.Guid} to url {template.Callback} threw exception {ex.Message}", ex);
                // silently swallow the exception - this is a background thread.
            }
        }
    }
}