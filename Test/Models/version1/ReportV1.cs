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
using System.Xml.Serialization;

namespace RESTfulEngine.Models.version1
{
	/// <summary>
	/// The generated report we are sending back to the client. Same as Document but v1 wants the root
	/// node to be <Report>.
	/// </summary>
	[XmlType(TypeName = "Report")]
	public class ReportV1
	{
		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public ReportV1(Document src)
		{
			Guid = src.Guid;
			Data = src.Data;
			Pages = src.Pages;
			Errors = src.Errors;
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public ReportV1()
		{
		}

		[DataMember]
		public string Guid { get; set; }

		/// <summary>
		/// The generated report in the user specified format.
		/// </summary>
		[DataMember]
		public byte[] Data { get; set; }

		/// <summary>
		/// A list of generated pages.  This is produced by the image report generator.
		/// </summary>
		[DataMember]
		public byte[][] Pages { get; set; }

		/// <summary>
		/// Contains a list of issues (errors and warnings) found during the report generation.
		/// The list is populating only if the error handling and verify is enabled.
		/// </summary>
		public Issue[] Errors { get; set; }
	}
} 
