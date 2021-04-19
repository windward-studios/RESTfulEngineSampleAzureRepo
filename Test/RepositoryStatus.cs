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

namespace RESTfulEngine.DocumentRepository
{
	/// <summary>
	/// The status of a request.
	/// </summary>
	public class RepositoryStatus
	{
		/// <summary>
		/// The job state (status)
		/// </summary>
		public enum JOB_STATUS
		{
			/// <summary>
			/// A lock file (not really a job).
			/// </summary>
			Lock,
			/// <summary>
			/// Waiting to run.
			/// </summary>
			Pending,
			/// <summary>
			/// Presently running.
			/// </summary>
			Generating,
			/// <summary>
			/// Request is complete, successful.
			/// </summary>
			Complete,
			/// <summary>
			/// Request had an error. Complete but failed.
			/// </summary>
			Error
		}

		/// <summary>
		/// What type of job. All have a Template object as their starting point.
		/// </summary>
		public enum REQUEST_TYPE
		{
			/// <summary>
			/// Should only be set for JOB_STATUS == Error
			/// </summary>
			Unknown,
			/// <summary>
			/// Generating a document.
			/// </summary>
			DocGen,
			/// <summary>
			/// Calling GetMetrics.
			/// </summary>
			Metrics,
			/// <summary>
			/// Calling GetTagTree.
			/// </summary>
			TagTree,
		}

		/// <summary>
		/// For a given status, return the string equivalent. Needed because the enum gets obfuscated.
		/// </summary>
		/// <param name="status">The job status.</param>
		/// <returns>The status as a lower case string.</returns>
		public static string JobStatusToString(JOB_STATUS status)
		{
			switch (status)
			{
				case JOB_STATUS.Lock:
					return "lock";
				case JOB_STATUS.Pending:
					return "pending";
				case JOB_STATUS.Generating:
					return "generating";
				case JOB_STATUS.Complete:
					return "complete";
				case JOB_STATUS.Error:
					return "error";
			}
			throw new ApplicationException($"Unknown status {status}");
		}

		/// <summary>
		/// For a given string, return the matching job status. Needed because the enum gets obfuscated.
		/// </summary>
		/// <param name="strJobStatus">The status as a lower case string.</param>
		/// <returns>The job status.</returns>
		public static JOB_STATUS StringToJobStatus(string strJobStatus)
		{
			strJobStatus = strJobStatus.ToLower().Trim();
			if (strJobStatus.StartsWith("."))
				strJobStatus = strJobStatus.Substring(1);
			switch (strJobStatus)
			{
				case "lock":
					return JOB_STATUS.Lock;
				case "pending":
					return JOB_STATUS.Pending;
				case "generating":
					return JOB_STATUS.Generating;
				case "complete":
					return JOB_STATUS.Complete;
				case "error":
					return JOB_STATUS.Error;
			}
			throw new ApplicationException($"Unknown extension {strJobStatus}");
		}

		/// <summary>
		/// For a given request type, return the string equivalent. Needed because the enum gets obfuscated.
		/// </summary>
		/// <param name="type">The request type.</param>
		/// <returns>The request type as a lower case string.</returns>
		public static string RequestTypeToString(REQUEST_TYPE type)
		{
			switch (type)
			{
				case REQUEST_TYPE.Unknown:
					return "unknown";
				case REQUEST_TYPE.DocGen:
					return "docgen";
				case REQUEST_TYPE.Metrics:
					return "metrics";
				case REQUEST_TYPE.TagTree:
					return "tagtree";
			}
			throw new ApplicationException($"Unknown type {type}");
		}

		/// <summary>
		/// For a given string return the matching request type. Needed because the enum gets obfuscated.
		/// </summary>
		/// <param name="strRequestType">The request type as a lower case string.</param>
		/// <returns>The request typen.</returns>
		public static REQUEST_TYPE StringToRequestType(string strRequestType)
		{
			strRequestType = strRequestType.ToLower().Trim();
			if (strRequestType.StartsWith("."))
				strRequestType = strRequestType.Substring(1);
			switch (strRequestType)
			{
				case "unknown":
					return REQUEST_TYPE.Unknown;
				case "docgen":
					return REQUEST_TYPE.DocGen;
				case "metrics":
					return REQUEST_TYPE.Metrics;
				case "tagtree":
					return REQUEST_TYPE.TagTree;
			}
			throw new ApplicationException($"Unknown extension {strRequestType}");
		}
	}
}