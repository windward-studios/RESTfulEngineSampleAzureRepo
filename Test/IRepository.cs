/*
 * Copyright (c) 2020 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

using RESTfulEngine.BusinessLogic;
using RESTfulEngine.Models;

namespace RESTfulEngine.DocumentRepository
{
	/// <summary>
	/// The RESTful engine stores requests and generated reports solely via this API. This API can be implemented by any
	/// system that wants to handle queueing requests and saving generated reports.
	///
	/// The instance of this used in the RESTful engine is a singleton.
	/// </summary>
	public interface IRepository
	{
		/// <summary>
		/// Give it the repository at startup. Only call this once.
		/// </summary>
		/// <param name="handler">The job handler that will process requests in this repository.</param>
		void SetJobHandler(IJobHandler handler);

		/// <summary>
		/// Revert all job in process to pending (waiting to start). The web server is closing down.
		/// </summary>
		void ShutDown();

		/// <summary>
		/// Add a request to generate a document, tagtree, etc.
		/// </summary>
		/// <param name="template">The template for the request.</param>
		/// <param name="requestType">What type of job is this.</param>
		/// <returns>The guid identifying this request.</returns>
		string CreateRequest(Template template, RepositoryStatus.REQUEST_TYPE requestType);

		/// <summary>
		/// Get a pending request. Returns null if there's no pending request. This will mark this request as now in a processing
		/// job. CompletedJob() must be called when this is complete.
		/// </summary>
		/// <returns>The template to process, with the guid identifying this request populated.</returns>
		RepositoryRequest TakeRequest();

		/// <summary>
		/// Save a completed docgen report.
		/// </summary>
		/// <param name="template">The template for the request.</param>
		/// <param name="document">The generated report.</param>
		void SaveReport(Template template, Document document);

		/// <summary>
		/// Save the error if a request threw an exception.
		/// </summary>
		/// <param name="template">The template for the request.</param>
		/// <param name="error">The error to return.</param>
		void SaveError(Template template, ServiceError error);

		/// <summary>
		/// Save a completed tag tree.
		/// </summary>
		/// <param name="template">The template for the request.</param>
		/// <param name="tree">The generated tag tree.</param>
		void SaveTagTree(Template template, TagTree tree);

		/// <summary>
		/// Save a completed metrics request.
		/// </summary>
		/// <param name="template">The template for the request.</param>
		/// <param name="metrics">The generated metrics.</param>
		void SaveMetrics(Template template, Metrics metrics);

		/// <summary>
		/// Get the status of this request.
		/// </summary>
		/// <param name="guid">The unique identifier for this request.</param>
		/// <returns>The status of the request.</returns>
		RequestStatus GetReportStatus(string guid);

		/// <summary>
		/// Get the generated report.
		/// </summary>
		/// <param name="guid">The unique identifier for this request.</param>
		/// <returns>The generated report.</returns>
		Document GetReport(string guid);

		/// <summary>
		/// Get the specified request's error.
		/// </summary>
		/// <param name="guid">The unique identifier for this request.</param>
		/// <returns>The error that occured trying to generate this report.</returns>
		ServiceError GetError(string guid);

		/// <summary>
		/// Get the generated tag tree.
		/// </summary>
		/// <param name="guid">The unique identifier for this request.</param>
		/// <returns>The generated tag tree.</returns>
		TagTree GetTagTree(string guid);

		/// <summary>
		/// Get the generated metrics.
		/// </summary>
		/// <param name="guid">The unique identifier for this request.</param>
		/// <returns>The generated metrics.</returns>
		Metrics GetMetrics(string guid);

		/// <summary>
		/// Delete the generated report. After this call, this request & result will no longer be in the repository.
		/// </summary>
		/// <param name="guid">The unique identifier for this request.</param>
		void DeleteReport(string guid);
	}
}
