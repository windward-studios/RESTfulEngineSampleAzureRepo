/*
 * Copyright (c) 2015-2017 by Windward Studios, Inc. All rights reserved.
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
	/// The generated document we are sending back to the client.
	/// </summary>
	[DataContract]
	public class Document
	{
		/// <summary>
		/// The unique identifier for this request.
		/// </summary>
		[DataMember]
		public string Guid { get; set; }

		/// <summary>
		/// The generated report as a single file in the user specified format. If this is populated Pages will be null.
		/// </summary>
		[DataMember]
		public byte[] Data { get; set; }

		/// <summary>
		/// The generated report as a distinct file per page in the user specified format. If this is populated Data will
		/// be null. This is produced by the image report generator and by the HTML report generator when it is in per page mode.
		/// </summary>
		[DataMember]
		public byte[][] Pages { get; set; }

		/// <summary>
		/// The number of pages in the generated document.
		/// </summary>
		[DataMember]
		public int NumberOfPages { get; set; }

		/// <summary>
		/// Anything you want. This is passed in to the repository & job handlers and is copied from the initial
		/// Template object to set in this object. The RESTful engine ignores this setting, it is for the caller's use.
		/// </summary>
		[DataMember]
		public string Tag { get; set; }

		/// <summary>
		/// The info on each import processed generating the document. The list is populating only if the ImportInfo enabled.
		/// </summary>
		[DataMember]
		public ImportMetrics[] ImportInfo { get; set; }

		/// <summary>
		/// Contains a list of issues (errors and warnings) found during the report generation.
		/// The list is populating only if the error handling and verify is enabled.
		/// </summary>
		[DataMember]
		public Issue[] Errors { get; set; }
	}
}
