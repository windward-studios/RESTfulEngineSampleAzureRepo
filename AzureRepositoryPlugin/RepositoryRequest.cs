/*
 * Copyright (c) 2020 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

using RESTfulEngine.Models;

namespace RESTfulEngine.DocumentRepository
{
    /// <summary>
    /// A pending request.
    /// </summary>
	public class RepositoryRequest
	{
		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public RepositoryRequest(Template template, RepositoryStatus.REQUEST_TYPE type)
		{
			Template = template;
			Type = type;
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public RepositoryRequest(RepositoryRequest src)
		{
			Template = src.Template;
			Type = src.Type;
		}

		/// <summary>
		/// The template that is the request.
		/// </summary>
		public Template Template { get; }

        /// <summary>
        /// The request type.
        /// </summary>
		public RepositoryStatus.REQUEST_TYPE Type { get; }

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return $"Type={Type}, Template={Template}";
		}
	}
}