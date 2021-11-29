using AzureRepositoryPlugin;
using AzureRepositoryPlugin.AzureStorage;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using WindwardModels;
using WindwardRepository;

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

        private static readonly ILog Log = LogManager.GetLogger("PluginLogger");

        private bool shutDown;
        private bool runningOnIIS;

        private StorageManager StorageManager;

        public AzureRepositoryPlugin()
        {
            runningOnIIS = HostingEnvironment.IsHosted;
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

            // Handle deleting old jobs
            if(runningOnIIS)
                HostingEnvironment.QueueBackgroundWorkItem(ct => ManageOldRequests(ct));
            else
            {
                var tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                Task.Run(() => ManageOldRequests(token), token);
            }

            Log.Info("[AzureRepoPlugin] AzureRepositoryPlugin constructor finished");
        }

        public string CreateRequest(Template template, RepositoryStatus.REQUEST_TYPE requestType)
        {
            Log.Info("[AzureRepoPlugin] Create REquest Called");
            try
            {
                var task = Task.Run(async () => await CreateRequestAsync(template, requestType));
                task.Wait();
                string guid = task.Result;
                return guid;
            } catch(Exception e)
            {
                Log.Error($"[AzureRepoPlguin] Exception in CreateRequest: {e.Message}");
                return null;
            }
        }

        public async Task<string> CreateRequestAsync(Template template, RepositoryStatus.REQUEST_TYPE requestType)
        {
            template.Guid = Guid.NewGuid().ToString();

            JobRequestData jobData = new JobRequestData
            {
                Template = template,
                RequestType = requestType,
                Action = JobRequestAction.CREATE,   //idk if we need this property at all
                CreationDate = DateTime.UtcNow
            };

            Log.Info($"[AzureRepoPlugin] Created request {jobData.Template.Guid}");

            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            bool success = await storage.AddRequest(jobData);

            if (!success)
            {
                Log.Error($"Failed to add job request [{jobData.Template.Guid}] to storage");
                return null;
            }

            Log.Info($"[AzureRepoPlugin] Added job request [{jobData.Template.Guid}] to storage");

            JobHandler?.Signal();

            return template.Guid;
        }

        public RepositoryRequest TakeRequest()
        {
            Log.Info("[AzureRepoPlugin] Take request called");
            try
            {
                var task = Task.Run(async () => await TakeRequestAsync());
                task.Wait();
                RepositoryRequest ret = task.Result;
                Log.Info($"[AzureRepoPlugin] Take request returned {ret}");

                return ret;
            } catch(Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in TakeRequest: {e.Message}");
                return null;
            }
        }

        private async Task<RepositoryRequest> TakeRequestAsync()
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            Log.Info($"[AzureRepoPlugin] Log storage is null: {storage == null}");
            JobRequestData job = await storage.GetOldestPendingJobAndGenerate();

            if (job != null)
                Log.Info($"[AzureRepoPlugin] Took reqest {job.Template.Guid}");
            else
                Log.Info($"[AzureRepoPlugin] Took request NULL");

            if (job == null)
                return null;

            return new RepositoryRequest(job.Template, job.RequestType);
        }

        public void DeleteReport(string guid)
        {
            try
            {
                var task = Task.Run(async () => await DeleteReportAsync(guid));
                task.Wait();
            } catch(Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Excepition in DeleteReport {e.Message}");
            }
        }

        private async Task<bool> DeleteReportAsync(string guid)
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            bool result = await storage.DeleteRequest(Guid.Parse(guid));
            return result;
        }

        public ServiceError GetError(string guid)
        {
            try
            {
                var task = Task.Run<ServiceError>(async () => await GetErrorAsync(guid));
                task.Wait();
                ServiceError res = task.Result;
                return res;
            } catch(Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in GetError: {e.Message}");
                return null;
            }
        }

        private async Task<ServiceError> GetErrorAsync(string guid)
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            var result = await storage.GetError(Guid.Parse(guid));
            return result;
        }

        public Metrics GetMetrics(string guid)
        {
            try
            {

            } catch(Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in GetMetrics: {e.Message}");
            }
            var task = Task.Run<Metrics>(async () => await GetMetricsAsync(guid));
            task.Wait();
            Metrics res = task.Result;
            return res;
        }

        private async Task<Metrics> GetMetricsAsync(string guid)
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            var result = await storage.GetMetrics(Guid.Parse(guid));
            return result;
        }

        public Document GetReport(string guid)
        {
            try
            {
                var task = Task.Run<Document>(async () => await GetReportAsync(guid));
                task.Wait();
                Document res = task.Result;
                return res;
            } catch(Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in GetReport: {e.Message}");
                return null;
            }
        }

        private async Task<Document> GetReportAsync(string guid)
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            var result = await storage.GetGeneratedReport(Guid.Parse(guid));
            return result;
        }

        public RequestStatus GetReportStatus(string guid)
        {
            try
            {
                AzureStorageManager storage = StorageManager.GetAzureStorageManager();
                JobInfoEntity result = storage.GetRequestInfo(Guid.Parse(guid));
                return new RequestStatus((RepositoryStatus.JOB_STATUS)result.Status, (RepositoryStatus.REQUEST_TYPE)result.Type);
            } catch(Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in GetReportStatus: {e.Message}");
                return null;
            }
        }

        public TagTree GetTagTree(string guid)
        {
            try
            {
                var task = Task.Run<TagTree>(async () => await GetTagTreeAsync(guid));
                task.Wait();
                TagTree res = task.Result;
                return res;
            } catch(Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in GetTagTree: {e.Message}");
                return null;
            }
        }

        private async Task<TagTree> GetTagTreeAsync(string guid)
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            var result = await storage.GetTagTree(Guid.Parse(guid));
            return result;
        }
        public DocumentMeta GetReportMeta(string guid)
        {
            try
            {
                var task = Task.Run<DocumentMeta>(async () => await GetReportMetaAsync(guid));
                task.Wait();
                DocumentMeta res = task.Result;
                return res;
            } catch(Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in GetReportMeta: {e.Message}");
                return null;
            }
        }

        public async Task<DocumentMeta> GetReportMetaAsync(string guid)
        {

            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            Document doc = await storage.GetGeneratedReport(Guid.Parse(guid));
            DocumentMeta ret = SetReportMeta(doc);
            return ret;
        }

        public void SaveError(Template template, ServiceError error)
        {
            try
            {
                var task = Task.Run(async () => await SaveErrorAsync(template, error));
                task.Wait();
                CompleteJob(template);
            } catch(Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in SaveError: {e.Message}");
            }
        }

        private async Task SaveErrorAsync(Template template, ServiceError error)
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            var result = await storage.UpdateRequest(Guid.Parse(template.Guid), RepositoryStatus.JOB_STATUS.Error);
            if (!result)
                Log.Error($"Failed to save error status {template.Guid}");

            result = await storage.CompleteRequest(Guid.Parse(template.Guid), error);
            if (result)
                Log.Debug($"[AzureRepoPlugin] Successfully saved error {template.Guid}");
            else
                Log.Error($"Failed to save error {template.Guid}");
        }

        public void SaveMetrics(Template template, Metrics metrics)
        {
            try
            {
                var task = Task.Run(async () => await SaveMetricsAsync(template, metrics));
                task.Wait();
                CompleteJob(template);
            }
            catch (Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in SaveMetrics: {e.Message}");
            }
        }

        private async Task SaveMetricsAsync(Template template, Metrics metrics)
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            var result = await storage.CompleteRequest(Guid.Parse(template.Guid), metrics);
            if (result)
                Log.Debug($"[AzureRepoPlugin] Successfully saved metrics {template.Guid}");
            else
                Log.Error($"Failed to save metrics {template.Guid}");
        }

        public void SaveReport(Template template, Document document)
        {
            try
            {
                var task = Task.Run(async () => await SaveReportAsync(template, document));
                task.Wait();
                CompleteJob(template);
            }
            catch (Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in SaveReport: {e.Message}");
            }
        }

        private async Task SaveReportAsync(Template template, Document document)
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            var result = await storage.CompleteRequest(Guid.Parse(template.Guid), document);
            if (result)
                Log.Debug($"[AzureRepoPlugin] Successfully saved document {template.Guid}");
            else
                Log.Error($"Failed to save document {template.Guid}");
        }

        public void SaveTagTree(Template template, TagTree tree)
        {
            try
            {
                var task = Task.Run(async () => await SaveTagTreeAsync(template, tree));
                task.Wait();
                CompleteJob(template);
            }
            catch (Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in SaveTagTree: {e.Message}");
            }
        }

        private async Task SaveTagTreeAsync(Template template, TagTree tree)
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            var result = await storage.CompleteRequest(Guid.Parse(template.Guid), tree);
            if (result)
                Log.Debug($"[AzureRepoPlugin] Successfully saved tag tree {template.Guid}");
            else
                Log.Error($"Failed to save tag tree {template.Guid}");
        }

        public void SetJobHandler(IJobHandler handler)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"[AzureRepoPlugin] SetJobHandler({handler})");
            JobHandler = handler;
        }

        public void ShutDown()
        {
            Log.Debug("[AzureRepoPlugin] AzureRepositoryPlugin.ShutDown() started...");
            shutDown = true;
            //_producer.Stop();

            Log.Debug("[AzureRepoPlugin] AzureRepositoryPlugin bus stopped");

            // Need to set all generating requests in azure storage back to pending
            var task = Task.Run(async () => await ShutDownAsync());
            task.Wait();
        }

        private async Task<bool> ShutDownAsync()
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            bool success = await storage.RevertGeneratingJobsToPending();
            if (success)
                Log.Debug("[AzureRepoPlugin] All generating jobs reverted to pending");
            return success;
        }

        private void DeleteOldJobs(DateTime cutoff)
        {
            try
            {
                var task = Task.Run(async () => await DeleteOldJobsAsync(cutoff));
                task.Wait();
            }
            catch (Exception e)
            {
                Log.Error($"[AzureRepoPlugin] Exception in DeleteOldJobs: {e.Message}");
            }
        }

        private async Task DeleteOldJobsAsync(DateTime cutoff)
        {
            AzureStorageManager storage = StorageManager.GetAzureStorageManager();
            var result = await storage.DeleteOldRequests(cutoff);
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
                        Log.Info($"[AzureRepoPlugin] Callback to {url} returned status code {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Log.Warn($"Callback for job {template.Guid} to url {template.Callback} threw exception {ex.Message}", ex);
                // silently swallow the exception - this is a background thread.
            }
        }

        private void ManageOldRequests(CancellationToken ct)
        {
            while ((!shutDown) && (!ct.IsCancellationRequested))
            {
                if (datetimeLastCheckOldJobs + timeSpanCheckOldJobs < DateTime.Now)
                {
                    Log.Info("[AzureRepoPlugin] Deleting old jobs");
                    DateTime cutoff = DateTime.Now - timeSpanDeleteOldJobs;
                    DeleteOldJobs(cutoff);
                    datetimeLastCheckOldJobs = DateTime.Now;
                }

                // wait until needed again, or cancelled, or time to check for jobs.
                WaitHandle.WaitAny(new WaitHandle[] { ct.WaitHandle }, timeSpanCheckOldJobs, false);
            }
            if (Log.IsDebugEnabled)
                Log.Debug("[AzureRepoPlugin] FileSystemRepository management worker stopped");
        }

        private DocumentMeta SetReportMeta(Document genDoc)
        {
            DocumentMeta largeDoc = new DocumentMeta();
            largeDoc.Guid = genDoc.Guid;
            largeDoc.NumberOfPages = genDoc.NumberOfPages;
            largeDoc.ImportInfo = genDoc.ImportInfo;
            largeDoc.Tag = genDoc.Tag;
            largeDoc.Errors = genDoc.Errors;

            if (genDoc.Pages == null)
            {
                Uri url = HttpContext.Current.Request.Url; ;
                string tempUri = url.AbsoluteUri.ToString();
                tempUri = tempUri.Substring(0, tempUri.Length - 4);
                largeDoc.Uri = tempUri + "file";
            }

            return largeDoc;
        }
    }
}