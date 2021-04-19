/*
* Copyright (c) 2015-2017 by Windward Studios, Inc. All rights reserved.
*
* This software is the confidential and proprietary information of
* Windward Studios ("Confidential Information").  You shall not
* disclose such Confidential Information and shall use it only in
* accordance with the terms of the license agreement you entered into
* with Windward Studios, Inc.
*/

using net.windward.api.csharp;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using WindwardReport = net.windward.api.csharp.Report;

namespace RESTfulEngine.Models
{
    /// <summary>
    /// The metrics from a template. This is the metadata stored in the template and within the tags.
    /// </summary>
    [DataContract]
    public partial class Metrics
    {
		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public Metrics()
		{
		}

		internal Metrics(Template template, TemplateMetrics src)
		{
			Guid = template.Guid;
			Tag = template.Tag;
			TemplateType = TemplateTypeToString(src.TemplateType);
			Datasources = src.Datasources.ToArray();
			Vars = src.Vars.ToArray();
			Variables = MakeVariablesArray(src.TemplateVariables);
			DatasourceProfiles = MakeTemplateDatasourcesArray(src.DataSourceProfiles);
			AutotagVersion = src.AutotagVersion;
		}

		/// <summary>
		/// The guid of this async job.
		/// </summary>
		[DataMember]
		public string Guid { get; set; }

        /// <summary>
		/// Anything you want. This is passed in to the repository & job handlers and is set in the final generated
		/// Report object. The RESTful engine ignores this setting, it is for the caller's use.
		/// </summary>
        [DataMember]
		public string Tag { get; set; }

        ///<summary>
        /// The format of the template. Allowed values are: DOCX, DOCM, HTML, PPTX, PPTM, XLSX, & XLSM
        ///</summary>
        [DataMember]
        public string TemplateType { get; set; }

		/// <summary>
		/// All datasources that must be processed for this template.
		/// </summary>
        [DataMember]
        public string[] Datasources { get; set; }

		/// <summary>
		/// All vars that must be defined by a caller in the template.
		/// </summary>
        [DataMember]
        public string[] Vars { get; set; }

		/// <summary>
		/// All of the template variables defined in the metadata.
		/// </summary>
        [DataMember]
        public Variable[] Variables { get; set; }

		/// <summary>
		/// All the datasources in the template's metadata. The properties are the set of name/value pairs that define the
		/// datasource. All include name, root-path, full-type, and windows-identity or username and password. XML includes
		/// url and schema. SQL includes provider-class and connection-string or server and database. There are additional
		/// properties for various cases (such as ODBC which has a provider).
		/// Many properties (like name) can have no value and then will not be in the returned Dictionary.
		/// </summary>
        [DataMember]
        public DatasourceProfile[] DatasourceProfiles { get; set; }

		/// <summary>
		/// The version of the metadata. null if no metadata.
		/// </summary>
        [DataMember]
        public string AutotagVersion { get; set; }


		private static string TemplateTypeToString(WindwardReport.TEMPLATE_TYPE type)
        {
            switch (type)
            {
                case WindwardReport.TEMPLATE_TYPE.DOCX:
                case WindwardReport.TEMPLATE_TYPE.DOCM:
                    return "docx";
                case WindwardReport.TEMPLATE_TYPE.HTML:
                    return "html";
                case WindwardReport.TEMPLATE_TYPE.PPTX:
                case WindwardReport.TEMPLATE_TYPE.PPTM:
                    return "pptx";
                case WindwardReport.TEMPLATE_TYPE.UNKNOWN:
                    return "unknown";
                case WindwardReport.TEMPLATE_TYPE.XLSX:
                case WindwardReport.TEMPLATE_TYPE.XLSM:
                    return "xlsx";
                default:
                    return "";
            }
        }

        private static Variable[] MakeVariablesArray(IList<TemplateVariable> vars)
        {
            List<Variable> allVars = new List<Variable>();

            foreach (var v in vars)
            {
                allVars.Add(Variable.Create(v));
            }

            return allVars.ToArray();
        }


		private static DatasourceProfile[] MakeTemplateDatasourcesArray(List<TemplateMetrics.DataSourceProfile> datasources)
        {
            List<DatasourceProfile> allEntries = new List<DatasourceProfile>();

            foreach (var d in datasources)
            {
                var datasource = d.Properties;
				DatasourceProfile tds = new DatasourceProfile();
                List<Entry> entries = new List<Entry>();
                foreach (var entry in datasource)
                {
                    entries.Add(new Entry() { Key = entry.Key, Value = entry.Value });
                }
                entries.Add(new Entry()
                {
                    Key = "full-type",
                    Value = d.VendorType
                });
                string simpleType = "";
                switch (d.VendorType)
                {
                    case "AdoDataSourceInfo":
                        simpleType = "sql";
                        break;
                    case "JsonDataSourceInfo":
                        simpleType = "json";
                        break;
                    case "ODataSourceInfo":
                        simpleType = "odata";
                        break;
                    case "XmlDataSourceInfo":
                        simpleType = "xml";
                        break;
                    case "SaxonDataSourceInfo":
                        simpleType = "xml2";
                        break;
                    case "SFDataSourceInfo":
                        simpleType = "salesforce";
                        break;
                    default:
                        break;
                }
                entries.Add(new Entry()
                {
                    Key = "simple-type",
                    Value = simpleType
                });
                entries.Add(new Entry(){
                    Key = "datasource-name",
                    Value = d.Name
                });
                tds.Properties = entries.ToArray();
                allEntries.Add(tds);
            }

            return allEntries.ToArray();
        }
    }
}
