/*
 * Copyright (c) 2015-2017 by Windward Studios, Inc. All rights reserved.
 *
 * This software is the confidential and proprietary information of
 * Windward Studios ("Confidential Information").  You shall not
 * disclose such Confidential Information and shall use it only in
 * accordance with the terms of the license agreement you entered into
 * with Windward Studios, Inc.
 */

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using log4net;
using net.windward.util.AccessProviders;
using RESTfulEngine.Models.version1;

namespace RESTfulEngine.Models
{

	/// <summary>
	/// The template contains all information required to generate a document. This is the body of a request that starts the generation.
	/// </summary>
	[DataContract]
    public class Template
    {
		private int trackErrors;

		private static readonly ILog Log = LogManager.GetLogger(typeof(Template));

		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public Template()
		{
			Properties = new Property[0];
			Parameters = new Parameter[0];
			Datasources = new Datasource[0];
		}

		public Template(TemplateV1 src)
		{
			trackErrors = src.TrackErrors;
			OutputFormat = src.OutputFormat;

			Data = src.Data;
			if (! string.IsNullOrEmpty(src.Uri))
				ConnectionString = $"{BaseAccessProvider.CONNECTION_URL}={src.Uri}";
			if (Log.IsDebugEnabled)
				Log.Debug($"src.Uri = {src.Uri}; ConnectionString = {ConnectionString}; Data[len] = {(Data == null ? "mull" : Convert.ToString(Data.Length))}");

			EngineVersion = src.EngineVersion;
			Format = src.Format;
			Version = src.Version;
			TrackImports = src.TrackImports;
			Timeout = src.Timeout;
			TrackErrors = src.TrackErrors;

			MainPrinter = src.MainPrinter;
			FirstPagePrinter = src.FirstPagePrinter;
			PrinterJobName = src.PrinterJobName;
			PrintCopies = src.PrintCopies;
			PrintDuplex = src.PrintDuplex;

			// first from Template (beloe from datasource)
			List<Parameter> vars = new List<Parameter>();
			if (src.Parameters != null)
				foreach (Variable var in src.Parameters)
					vars.Add(new Parameter(var.Name, var.Value));

			// need to convert datasources
			List<Datasource> datasources = new List<Datasource>();
			if (src.Datasources != null)
				foreach (DatasourceV1 srcDatasource in src.Datasources)
				{
					datasources.Add(new Datasource(srcDatasource));
					if (srcDatasource.Variables != null)
						foreach (Variable var in srcDatasource.Variables)
							vars.Add(new Parameter(var.Name, var.Value));
				}

			Datasources = datasources.ToArray();

			// add in params - from Template & Datasource(s)
			Parameters = vars.ToArray();

			// stuff all the explicit settings into properties
			// this works for overwriting existing values because these will be added to the actual report properties in order
			// so these at the end will overwrite and existing values.
			List<Property> props = new List<Property>();
			if (src.Dpi > 0)
				props.Add(new Property("default.image.dpi", Convert.ToString(src.Dpi)));
			if (!string.IsNullOrEmpty(src.CopyMetadata))
				props.Add(new Property("openxml.copy-metadata", copyMetadataMap[src.CopyMetadata]));
			if (!string.IsNullOrEmpty(src.Description))
				props.Add(new Property("report.description", src.Description));
			if (!string.IsNullOrEmpty(src.Title))
				props.Add(new Property("report.title", src.Title));
			if (!string.IsNullOrEmpty(src.Subject))
				props.Add(new Property("report.subject", src.Subject));
			if (!string.IsNullOrEmpty(src.Keywords))
				props.Add(new Property("report.keywords", src.Keywords));
			if (!string.IsNullOrEmpty(src.Locale))
				props.Add(new Property("report.locale", src.Locale));
			props.Add(new Property("report.remove-unused-formats", src.RemoveUnusedFormats ? "true" : "false"));
			if (!string.IsNullOrEmpty(src.Hyphenate))
				props.Add(new Property("report.hyphenate", src.Hyphenate));

			Properties = props.ToArray();
		}

		private static Dictionary<string, string> copyMetadataMap = new Dictionary<string, string>()
		{
			{"never", "no"},
			{"always", "yes"},
			{"nodatasource", "no-datasource"}
		};

		/// <summary>
		/// This is not passed up in the body. It is passed in the request header and then set in the controller. It is also
		/// not passed back to the caller.
		/// </summary>
		[DataMember]
		public string LicenseFromHeader { get; set; }

		/// <summary>
		/// The unique identifier for this request.
		/// </summary>
		[DataMember]
		public string Guid { get; set; }

		/// <summary>
		/// If set, this url will be called with a POST when a job completes. If the text "{guid}" is in the url, that text will
		/// be replaced with the Guid for the callback.
		/// </summary>
		[DataMember]
		public string Callback { get; set; }

        /// <summary>
        /// Specifies what format of the report to generate. The allowed values are:<br/>
        /// csv - Comma delimited<br/>
		/// docx<br/>
		/// html<br/>
		/// pdf<br/>
		/// ps - PostScript<br/>
		/// pptx<br/>
		/// prn - Send the output directly to a printer.<br/>
		/// rtf - Rich Text Format<br/>
		/// text - ASCII text<br/>
		/// xlsx<br/>
		/// bitmaps and images:<br/>
		/// bmp<br/>
		/// eps<br/>
		/// gif<br/>
		/// jpg<br/>
		/// png<br/>
		/// svg<br/>
		/// tif<br/>
        /// </summary>
        [DataMember]
        public string OutputFormat { get; set; }

        /// <summary>
        /// Set this to provide the template as a Base64 encoded binary.
        /// </summary>
        [DataMember]
        public byte[] Data { get; set; }

		/// <summary>
		/// Set this to provide the template as a connection string of the template's location. This is not just a
		/// filename. At a minimum it is "Url=filename;"
		/// </summary>
		[DataMember]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Set this to specify the version of the underlying engine to use. This value is advisory and can be ignored.
        /// </summary>
        [DataMember]
        public string EngineVersion { get; set; }

        /// <summary>
        /// The format of the template. The format is auto-determined if not provided. The allowed values are:<br/>
        /// docx<br/>
        /// pptx<br/>
        /// xlsx<br/>
        /// </summary>
        [DataMember]
        public string Format { get; set; }

        /// <summary>
        /// The version of the designer this template was created in. If null, assumes the most recent version.
        /// </summary>
        [DataMember]
        public string Version { get; set; }

		/// <summary>
		/// Windward properties for this report. These override any properties set in the configuration file on the server side.
		/// </summary>
		[DataMember]
		public Property[] Properties { get; set; }

        /// <summary>
        /// A set of input parameters for this report. The parameters are global and shared among all data sources.
        /// </summary>
        [DataMember]
        public Parameter[] Parameters { get; set; }

		/// <summary>
		/// The datasources to apply to the template. The datasources are applied simultaneously.
		/// </summary>
		[DataMember]
		public Datasource[] Datasources { get; set; }

        /// <summary>
        /// Anything you want. This is passed in to the repository and job handlers and is set in the final generated
        /// Report object. The RESTful engine ignores this setting, it is for the caller's use.
        /// </summary>
		[DataMember]
		public string Tag { get; set; }

		/// <summary>
        /// Return all imports with the generated document.
        /// </summary>
		[DataMember]
		public bool TrackImports { get; set; }

		/// <summary>
		/// How many seconds to timeout. If after this amount of time the report is still generating, it will fail with a timeout exception.
		/// </summary>
		[DataMember]
		public int Timeout { get; set; }

		/// <summary>
		/// Enable or disable the error handling and verify functionality.
		/// Available options are:<br/>
		/// 0 - error handling and verify is disabled.  This is the default.<br/>
		/// 1 - enable error handling.<br/>
		/// 2 - enable verify.<br/>
		/// 3 - enable both error handling and verify.<br/>
		/// Any other value is ignored and disables the functionality.
		/// </summary>
		[DataMember]
        public int TrackErrors
        {
            get
            {
                return trackErrors;
            }
            set
            {
                trackErrors = value;
                // Correction
                if (trackErrors < 0 || trackErrors > 3)
                    trackErrors = 0;
            }
        }

		/// <summary>
		/// A name of the printer to send the output to.  This is used as the main printer
		///
		/// The server, where the report generation is run, must have an access to this
		/// printer.
		/// </summary>
		[DataMember]
		public string MainPrinter { get; set; }

		/// <summary>
		/// A name of the printer to use for the first page of the report.  All subsequent pages
		/// will be sent to the main printer.
		///
		/// The server, where the report generation is run, must have an access to this
		/// printer.
		/// </summary>
		[DataMember]
		public string FirstPagePrinter { get; set; }

		/// <summary>
		/// A client defined name for the printing job.
		/// </summary>
		[DataMember]
		public string PrinterJobName { get; set; }

		/// <summary>
		/// The number of copies to print.  Must be greater than 0.  The default is 1.
		/// </summary>
		[DataMember]
		public int PrintCopies { get; set; }

		/// <summary>
		/// Selects the printer duplex mode.  Only if supported by the printer.
		///
		/// Valid value are
		///
		/// simplex
		///     One-sided printing.  This is the default.
		/// horizontal
		///     Two-sided printing.  Prints on both sides of the paper for portrait output.
		/// vertical
		///     Two-sided printing.  Prints on both sides of the paper for landscape output.
		/// </summary>
		[DataMember]
		public string PrintDuplex { get; set; }
     }

}
