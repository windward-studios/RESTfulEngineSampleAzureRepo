/*
 * Copyright (c) 2020 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

namespace RESTfulEngine.DocumentRepository
{
    /// <summary>
    /// The status of a request.
    /// </summary>
	public class RequestStatus
	{
		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public RequestStatus(RepositoryStatus.JOB_STATUS jobStatus, RepositoryStatus.REQUEST_TYPE requestType)
		{
			JobStatus = jobStatus;
			RequestType = requestType;
		}

        /// <summary>
        /// The request's job status
        /// </summary>
		public RepositoryStatus.JOB_STATUS JobStatus { get; }

        /// <summary>
        /// The request's type
        /// </summary>
		public RepositoryStatus.REQUEST_TYPE RequestType { get; }
	}
}