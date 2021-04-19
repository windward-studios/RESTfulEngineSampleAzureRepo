/*
* Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
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
	[DataContract]
	[XmlType (TypeName = "Datasource")]
	public class DatasourceV1
	{
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// A type of this data source.  Use one of the following:
		/// ado
		///     Can be used with any ADO.NET connector available on the server.  Requires to provide ClassName and ConnectionString properties.
		///     For example, to connect to an Excel data source, the properties could be set as follows:
		///         Type = ado
		///         ClassName = System.Data.OleDb
		///         ConnectionString = Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Northwind.xlsx;Extended Properties="Excel 12.0 Xml;HDR=YES"
		/// sql
		///     Synonym for 'ado'.
		/// xml2 (Saxon based xpath 2.0 implementation)
		/// xml (The old xpath 1.0)
		/// json
		/// odata
		/// salesforce
		/// </summary>
		[DataMember]
		public string Type { get; set; }

		[DataMember]
		public string ClassName { get; set; }

		[DataMember]
		public string ConnectionString { get; set; }

		[DataMember]
		public byte[] Data { get; set; }

		[DataMember]
		public string Uri { get; set; }

		[DataMember]
		public byte[] SchemaData { get; set; }

		[DataMember]
		public string SchemaUri { get; set; }

		/// <summary>
		/// Get or set the schema connection string.
		/// </summary>
		[DataMember]
		public string SchemaConnectionString { get; set; }

		[DataMember]
		public string Username { get; set; }

		[DataMember]
		public string Password { get; set; }

		[DataMember]
		public string Domain { get; set; }

		[DataMember]
		public string ODataVersion { get; set; }

		[DataMember]
		public string ODataProtocol { get; set; }

		[DataMember]
		public string SalesforceToken { get; set; }

		[DataMember]
		public string SalesforceAccessToken { get; set; }

		[DataMember]
		public string SalesforceSoapEndpoint { get; set; }

		/// <summary>
		/// A set of input parameters for this report.
		/// 
		/// The parameters are global and shared among all data sources.
		/// 
		/// This field marked obsolete in 20.0. Use Template.Parameters instead.
		/// </summary>
		[DataMember]
		public Variable[] Variables { get; set; }

		/// <summary>
		/// Test if this data source contains a connection string.
		/// </summary>
		/// <returns>True if this data soure contains a connection string.</returns>
		public bool HasConnectionString()
		{
			return ConnectionString != null && ConnectionString.Length > 0;
		}
	}
}