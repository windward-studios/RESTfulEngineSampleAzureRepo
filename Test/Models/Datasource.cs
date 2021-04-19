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
using System.Text;
//using Kailua.net.windward.utils;
using RESTfulEngine.Models.version1;

namespace RESTfulEngine.Models
{

	/// <summary>
	/// A datasource to apply to a template. This can include datasets built from this datasource (version 2 API).
	/// </summary>
	[DataContract]
	public class Datasource
	{
		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public Datasource()
		{
			Datasets = new DataSet[0];
		}

		public Datasource (DatasourceV1 src)
		{

			Name = src.Name;
			Type = src.Type;
			ClassName = src.ClassName;
			if (string.IsNullOrEmpty(src.ConnectionString) && !string.IsNullOrEmpty(src.Uri))
				ConnectionString = $"Url={src.Uri}";
			else
				ConnectionString = src.ConnectionString;
			Data = src.Data;
			SchemaData = src.SchemaData;
			Datasets = new DataSet[0];

			// only XML also has the schema connection string
			if (src.Type == "xml" || src.Type == "xml2")
			{
				if (string.IsNullOrEmpty(src.SchemaConnectionString) && !string.IsNullOrEmpty(src.SchemaUri))
					SchemaConnectionString = $"Url={src.SchemaUri}";
				else
					SchemaConnectionString = src.SchemaConnectionString;
			}

			if (IsConnectionString || Data != null)
				return;

			// build up the connection string
			StringBuilder buf = new StringBuilder($"Url={src.Uri};");
			if (!string.IsNullOrEmpty(src.Username))
				buf.Append($"Username={src.Username}");
			if (!string.IsNullOrEmpty(src.Password))
				buf.Append($"Password={src.Password}");
			if (!string.IsNullOrEmpty(src.Domain))
				buf.Append($"Domain={src.Domain}");
			if (!string.IsNullOrEmpty(src.ODataVersion))
				buf.Append($"Version={src.ODataVersion}");

			if (!string.IsNullOrEmpty(src.SalesforceToken))
				buf.Append($"Token={src.SalesforceToken}");
			else if (!string.IsNullOrEmpty(src.SalesforceAccessToken))
				buf.Append($"Token={src.SalesforceAccessToken}");
			if (!string.IsNullOrEmpty(src.SalesforceSoapEndpoint))
				buf.Append($"Endpoint={src.SalesforceSoapEndpoint}");

			// we're not doing anything with src.ODataProtocol because default is to map to AllHttp and that
			// should catch all 4 settings.

			ConnectionString = buf.ToString();
		}

		/// <summary>
		/// The datasource name which maps to the datasource attribute in tags.
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// A type of this data source.  Use one of the following:<br/>
		/// ado<br/>
		///     Can be used with any ADO.NET connector available on the server.  Requires to provide ClassName and ConnectionString properties.<br/>
		///     For example, to connect to an Excel data source, the properties could be set as follows:<br/>
		///         Type = ado<br/>
		///         ClassName = System.Data.OleDb<br/>
		///         ConnectionString = Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Northwind.xlsx;Extended Properties="Excel 12.0 Xml;HDR=YES"<br/>
		/// sql<br/>
		///     Synonym for 'ado'.<br/>
		/// xml2 (Saxon based xpath 2.0 implementation)<br/>
		/// xml (The old xpath 1.0)<br/>
		/// json<br/>
		/// odata<br/>
		/// salesforce
		/// </summary>
		[DataMember]
		public string Type { get; set; }

		/// <summary>
		/// The ADO.NET connector classname. null for other types.
		/// </summary>
		[DataMember]
		public string ClassName { get; set; }

		/// <summary>
		/// The connection string to the datasource. This is not just a filename. At a minimum it is "Url=filename;"
		/// </summary>
		[DataMember]
		public string ConnectionString { get; set; }

		/// <summary>
		/// The actual data (JSON and XML only). Used when passing up the actual data.
		/// </summary>
		[DataMember]
		public byte[] Data { get; set; }

		/// <summary>
		/// The schema (XML only). Used when passing up the actual data. The XML and schema must both be Data or both
		/// be a ConnectionString.
		/// </summary>
		[DataMember]
		public byte[] SchemaData { get; set; }

		/// <summary>
		/// The connection string to the the schema (XML only) file for XML. This is not just a filename. At a minimum
		/// it is "Url=filename;" The XML and schema must both be Data or both be a ConnectionString.
		/// </summary>
		/// <summary>
		/// </summary>
		[DataMember]
		public string SchemaConnectionString { get; set; }

		/// <summary>
		/// The datasets created on this datasource. This property is new to the version 2 API.
		/// </summary>
		[DataMember]
		public DataSet[] Datasets { get; set; }

		/// <summary>
		/// Test if this data source contains a connection string. False if the literal data is being passed
		/// </summary>
		/// <returns>True if this data source contains a connection string.</returns>
		public bool IsConnectionString => !string.IsNullOrEmpty(ConnectionString);
	}
}
