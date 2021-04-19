/*
 * Copyright (c) 2020 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */


using RESTfulEngine.DocumentRepository;

namespace RESTfulEngine.BusinessLogic
{
    /// <summary>
    /// Creates a singleton of this to run all background jobs. The actual class is created as a singleton.
    /// </summary>
	public interface IJobHandler
	{
        /// <summary>
        /// Give it the repository at startup. Only call this once.
        /// </summary>
        /// <param name="repository">The repository that the job handler pulls jobs from to process.</param>
		void SetRepository(IRepository repository);

        /// <summary>
        /// A pending job is probably available.
        /// </summary>
		void Signal();

        /// <summary>
        /// Shut down all jobs, do not start any new ones. The web server is closing down.
        /// </summary>
		void ShutDown();
	}
}
