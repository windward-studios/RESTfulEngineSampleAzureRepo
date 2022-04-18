/*
 * Copyright (c) 2020 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using log4net;
using RESTfulEngine.BusinessLogic;
using RESTfulEngine.DocumentRepository;
using WindwardRepository;

namespace AzureRepository
{
	/// <summary>
	/// Creates a QueueBackgroundWorkItem for each new job. It will start any pending jobs when created so jobs not
	/// started when the app was shutdown will be processed.
	/// It does not persist jobs in process. A job in process when the app is shut down (or recycled) is lost.
	/// It does not limit the number of threads active at once. It leaves that to IIS.
	/// </summary>
	public class CustomBackgroundWorkerJobHandler : IJobHandler
	{
		/// <summary>The repository holding all jobs.</summary>
		private IRepository repository;

		/// <summary>true if running on IIS. false if running otherwise (usually unit tests).</summary>
		private readonly bool runningOnIIS;

		/// <summary>Used by the main worker to wake it up.</summary>
		private AutoResetEvent eventSignal;

		/// <summary>true when we're shutting down.</summary>
		private bool shutDown;

		/// <summary>Every N minutes check for pending jobs. Should not be needed but paranoia isn't a bad thing.</summary>
		private TimeSpan timeSpanCheckForJobs;

		/// <summary>The maximum number of threads we'll run in the background.</summary>
		private readonly int maxThreads;

		/// <summary>
		/// The number of threads we have running. ONLY access this using Interlocked.Incrment/Decrement.
		/// </summary>
		private int numThreadsRunning;

		private ReportGenerator reportGen = new ReportGenerator();
		private MetricsGenerator metricsGen = new MetricsGenerator();
		private TagTreeGenerator tagTreeGen = new TagTreeGenerator();

		private static readonly ILog Log = LogManager.GetLogger("PluginLogger");

		/// <summary>Create the job handler system.</summary>
		public CustomBackgroundWorkerJobHandler()
		{
			runningOnIIS = HostingEnvironment.IsHosted;
			Log.Info($"[CustomBGWorker] starting CustomBackgroundWorkerJobHandler {(runningOnIIS ? "on IIS" : "on .Net")}");
		
			string num = ConfigurationManager.AppSettings["max.threads"] ?? "";
			if (!int.TryParse(num, out maxThreads))
				maxThreads = (int) (Environment.ProcessorCount * 1.5f);

			num = ConfigurationManager.AppSettings["minutes.check.jobs"] ?? "";
			if (!int.TryParse(num, out int minutes))
				minutes = 10;
			timeSpanCheckForJobs = TimeSpan.FromMinutes(minutes);

			Log.Info($"[CustomBGWorker] Maximum workers = {maxThreads}, Job check interval = {timeSpanCheckForJobs}");

			eventSignal = new AutoResetEvent(true);
		}

		/// <summary>
		/// Give it the repository at startup. Only call this once.
		/// </summary>
		/// <param name="repository">The repository that the job handler pulls jobs from to process.</param>
		public void SetRepository(IRepository repository)
		{

			if (Log.IsDebugEnabled)
				Log.Debug($"SetRepository({repository})");
			this.repository = repository;

			// This thread manages all the background threads. It sleeps on an event and when awoken, fires off anything it can.
			// This is used so web requests that call signal aren't delayed as background tasks might be started.
			if (runningOnIIS)
				HostingEnvironment.QueueBackgroundWorkItem(ct => ManageWorkers(ct));
			else
			{
				var tokenSource = new CancellationTokenSource();
				var token = tokenSource.Token;
				Task.Run(() => ManageWorkers(token), token);
			}
		}

		private void ManageWorkers(CancellationToken ct)
		{
			Log.Info("[CustomBGWorker] In ManageWorkers");
			while ((! shutDown) && (! ct.IsCancellationRequested))
			{
				StartWaitingJobs();

				// wait until needed again, or cancelled, or time to check for jobs.
				Log.Info($"[CustomBGWorker] Awaiting WaitHandler");
				int status = WaitHandle.WaitAny(new WaitHandle[] { eventSignal, ct.WaitHandle }, timeSpanCheckForJobs, false);
				Log.Info($"[CustomBGWorker] Status from WaitHandler: {status}");
			}
			if (Log.IsInfoEnabled)
				Log.Info("[CustomBGWorker] CustomBackgroundWorkerJobHandler management worker stopped");
		}

		/// <summary>
		/// See if a pending job is available.
		/// </summary>
		public void Signal()
		{
			bool success = eventSignal.Set();
			if (success)
				Log.Info($"[CustomBGWorker] Signal was called SUCCESSFULLY; threads available = {maxThreads - numThreadsRunning}");
			else
				Log.Warn($"[CustomBGWorker] Signal was called UNSUCCESSFULLY");
		}

		/// <summary>
		/// Shut down all jobs, do not start any new ones. The web server is closing down.
		/// </summary>
		public void ShutDown()
		{
			Log.Info("[CustomBGWorker] Shutdown was called");
			shutDown = true;
			eventSignal.Set();
		}

		/// <summary>
		/// See if a pending job is available.
		/// </summary>
		public void StartWaitingJobs()
		{
			Log.Info("[CustomBGWorker] StartWaitingJobs called");
			while (true)
			{
				if (shutDown)
				{
					Log.Info("[CustomBGWorker] Returned from StartWaitingJobs bc shutDown = true");
					return;
				}

				// We don't want to get a job unless there's an available thread. So we increment and if we're over the limit
				// decrement and return. But if we're good, then the increment is here and the decrement is in the worker thread.
				// we can't decrement here and then increment in the worker because that would leave a window.
				int numThreads = Interlocked.Increment(ref numThreadsRunning);
				Log.Info($"[CustomBGWorker] Num threads: {numThreads}");
				if (numThreads > maxThreads)
				{
					Log.Info($"[CustomBGWorker] NumThreads[{numThreads}] > MaxThreads [{maxThreads}], returning");
					Interlocked.Decrement(ref numThreadsRunning);
					return;
				}
				RepositoryRequest job = repository.TakeRequest();
				if (job == null)
				{
					Log.Info("[CustomBGWorker] Job from take request was NULL");
					Interlocked.Decrement(ref numThreadsRunning);
					return;
				}

				RepositoryRequest _job = new RepositoryRequest(job);
				if (Log.IsDebugEnabled)
					Log.Debug($"Signal launching task for job {_job.Template.Guid}");

				if (runningOnIIS)
					HostingEnvironment.QueueBackgroundWorkItem(ct => RunJobs(ct, _job));
				else
				{
					// we don't use this CancellationToken, this is called in Unit Tests. But need it as running under IIS it is used.
					var tokenSource = new CancellationTokenSource();
					var token = tokenSource.Token;
					Task.Run(() => RunJobs(token, _job), token);
				}
				Log.Info("[CustomBGWorker] Run Jobs method has finished. Back in StartWaitingJobs()");
			}
		}

		private void RunJobs(CancellationToken ct, RepositoryRequest job)
		{
			Log.Info("[CustomBGWorker] RunJobs was called");
			try
			{
				if (Log.IsDebugEnabled)
					Log.Debug($"[CustomBGWorker] Worker running job {job.Template.Guid}, numThreads={numThreadsRunning}");

				do
				{
					switch (job.Type)
					{
						case RepositoryStatus.REQUEST_TYPE.DocGen:
							reportGen.Generate(job.Template, repository);
							break;
						case RepositoryStatus.REQUEST_TYPE.Metrics:
							metricsGen.Metrics(job.Template);
							break;
						case RepositoryStatus.REQUEST_TYPE.TagTree:
							tagTreeGen.TagTree(job.Template);
							break;
						default:
							Log.Error($"Requested unknown job {job}");
							break;
					}

					if (Log.IsDebugEnabled)
						Log.Debug($"[CustomBGWorker] Worker completed job {job.Template.Guid}, numThreads={numThreadsRunning}");

					// when IIS is shutting down
					if (shutDown || ct.IsCancellationRequested)
					{
						if (Log.IsDebugEnabled)
							Log.Debug("[CustomBGWorker] worker is cancelled");
						return;
					}

					// did we go over the max due to several Signal() calls at the same time?
					// if so, end this worker
					if (numThreadsRunning > maxThreads)
					{
						if (Log.IsDebugEnabled)
							Log.Debug($"[CustomBGWorker] Too many workers: {numThreadsRunning} > {maxThreads}");
						return;
					}

					// another job available?
					Log.Info($"[CustomBGWorker] About to call TakeRequest in RunJobs");
					job = repository.TakeRequest();
					Log.Info($"[CustomBGWorker] TakeRequest in RunJobs finished.  Is job null: {job == null}");
					if (job != null && Log.IsDebugEnabled)
						Log.Debug($"[CustomBGWorker] Worker running additional job {job.Template.Guid}, numThreads={numThreadsRunning}");
				} while (job != null);
			}
			finally
			{
				if (Log.IsDebugEnabled)
					Log.Debug($"[CustomBGWorker] Exiting worker, numThreads={numThreadsRunning}");
				// it was incremented in StartWaitingJobs - and now we're done with it.
				Interlocked.Decrement(ref numThreadsRunning);
			}
		}
	}
}