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
	/// Information stored in the template for a datasource.
	/// </summary>
	[DataContract]
	public class DatasourceProfile
	{
		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public DatasourceProfile()
		{
			Properties = new Entry[0];
			Datasets = new DataSetProfile[0];
		}

		/// <summary>
		/// The name of the datasource. Will be the empty string for an unnamed datasource.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The root path applied to this datasource. Can be null.
		/// </summary>
		public string RootPath { get; }

		/// <summary>
		/// The AutoTag defined datasource type.
		/// </summary>
		public string VendorType { get; }

		/// <summary>
		/// All the properties set for this datasource.
		/// </summary>
		[DataMember]
		public Entry[] Properties { get; set; }

		/// <summary>
		/// The datasets in this datasource. Can be size 0 (no datasets).
		/// </summary>
		public DataSetProfile[] Datasets { get; }
	}
}