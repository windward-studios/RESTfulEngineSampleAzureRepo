/*
* Copyright (c) 2015-2017 by Windward Studios, Inc. All rights reserved.
*
* This software is the confidential and proprietary information of
* Windward Studios ("Confidential Information").  You shall not
* disclose such Confidential Information and shall use it only in
* accordance with the terms of the license agreement you entered into
* with Windward Studios, Inc.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
//using Kailua.net.windward.utils;

namespace RESTfulEngine.Models
{
	/// <summary>
	/// The info about an import processed generating the document.
	/// </summary>
    [DataContract]
	public class ImportMetrics
	{
		/// <summary>Empty constructor for (de)serialization.</summary>
		public ImportMetrics()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		//public ImportMetrics(net.windward.api.csharp.ImportMetrics  src)
		//{
		//	Type = src.Type.ToString();
		//	Tag = src.Tag;
		//	Filename = src.Filename;
		//	if (src.Children != null && src.Children.Count > 0)
		//		Children = Factory(src.Children);
		//}

		/// <summary>
		/// The type of imported file.
		/// </summary>
		[DataMember]
		public string Type { get; set; }

		/// <summary>
		/// The full import tag that imports this file.
		/// </summary>
		[DataMember]
		public string Tag { get; set; }

		/// <summary>
		/// The filename of the file imported.
		/// </summary>
        [DataMember]
		public string Filename { get; set; }

		/// <summary>
		/// The child imports of this imported template.
		/// </summary>
        [DataMember]
		public ImportMetrics[] Children { get; set; }

		/// <summary>
		/// Create the REST ImportMetrics from the engine ImportMetrics.
		/// </summary>
		/// <param name="generatedReportImportInfo">The engine ImportMetrics.</param>
		/// <returns>The built up list of metrics.</returns>
		//public static ImportMetrics[] Factory(ICollection<net.windward.api.csharp.ImportMetrics> generatedReportImportInfo)
		//{
		//	List<ImportMetrics> list = new List<ImportMetrics>();
		//	Trap.trap();
		//	foreach (net.windward.api.csharp.ImportMetrics metrics in generatedReportImportInfo)
		//		list.Add(new ImportMetrics(metrics));
		//	return list.ToArray();
		//}
	}
}