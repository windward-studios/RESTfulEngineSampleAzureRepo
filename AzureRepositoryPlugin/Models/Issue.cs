/*
* Copyright (c) 2017 by Windward Studios, Inc. All rights reserved.
*
* This software is the confidential and proprietary information of
* Windward Studios ("Confidential Information").  You shall not
* disclose such Confidential Information and shall use it only in
* accordance with the terms of the license agreement you entered into
* with Windward Studios, Inc.
*/

using System.Runtime.Serialization;

namespace RESTfulEngine.Models
{
    /// <summary>
    /// An issue found during the report generation.  Issues are creating
    /// if the error handling and verify functionality is enabled.
    /// The issue represents an error or a warning.
    /// </summary>
    [DataContract]
    public class Issue
    {
		/// <summary>
		/// Empty constructor. Need for (de)serialization.
		/// </summary>
		public Issue()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		//public Issue(net.windward.api.csharp.errorhandling.Issue src)
		//{
		//	Message = src.Message;
		//	IsError = src.IsError;
		//	IsWarning = src.IsWarning;
		//	IssueType = src.Type.ToString();
		//	Tag = src.Tag.FullTag;
		//}

		/// <summary>
        /// A textual description of this issue.
        /// </summary>
        [DataMember]
        public string Message { get; set; }

		/// <summary>
		/// Test if this issue is an error.
		/// </summary>
		[DataMember]
        public bool IsError { get; set; }

		/// <summary>
		/// Test if this issue is a warning.
		/// </summary>
		[DataMember]
		public bool IsWarning { get; set; }

		/// <summary>
		/// A type of this issue.
		/// </summary>
		[DataMember]
		public string IssueType { get; set; }

		/// <summary>
		/// A tag that led to this issue.  This is the tag from the source template.
		/// </summary>
		[DataMember]
		public string Tag { get; set; }
	}
}