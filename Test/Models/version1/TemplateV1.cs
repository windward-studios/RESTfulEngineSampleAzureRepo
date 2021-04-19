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
	/// A template provided by the client.  From this template we are generating the report.
	/// </summary>
	[XmlType (TypeName ="Template")]
	public class TemplateV1
	{
		/// <summary>
		/// Base64 encoded template file.
		/// </summary>
		[DataMember]
		public byte[] Data { get; set; }

		// Uri of a template location
		[DataMember]
		public string Uri { get; set; }

		/// <summary>
		/// Tells what format of the report to generate.
		///
		/// The known formats are
		///
		/// pdf
		///     Portable document format.
		/// docx
		///     Microsoft Word document.
		/// xlsx
		///     Microsoft Excel document.
		/// pptx
		///     Microsoft PowerPoint document.
		/// html
		///     HTML file.
		/// csv
		///     Coma delimited format.
		/// rtf
		///     Rich-text format.
		/// prn
		///     Send the output directly to a printer.  Make sure that the server,
		///     where the report is generating, has access to that printer.
		/// ps
		///     PostScript document
		///
		/// The following image output formats can be specified.
		///
		/// png
		/// jpg
		/// bmp
		/// gif
		/// svg
		/// eps
		/// </summary>
		[DataMember]
		public string OutputFormat { get; set; }

		/// <summary>
		/// A set of input parameters for this report.
		/// 
		/// The parameters are global and shared among all data sources.
		/// </summary>
		[DataMember]
		public Variable[] Parameters { get; set; }

		/// <summary>
		/// DPI for the bitmap images (BMP, GIF, JPG, PNG).
		/// 72 by default.
		/// </summary>
		[DataMember]
		public int Dpi { get; set; }

		// Do not wait till a report completes. Return a GUID of the report being generated immediately.
		[DataMember]
		public bool Async { get; set; }

		// This is for Scout installations only.
		[DataMember]
		public string ApiKey { get; set; }

		// A version of the engine to use.
		[DataMember]
		public string EngineVersion { get; set; }

		// Format of the template.  Auto-determined if not provided.
		// docx|xlsx|pptx
		[DataMember]
		public string Format { get; set; }

		// Template's version.
		[DataMember]
		public string Version { get; set; }

		// Datasource descriptions.  Apply all in order.
		[DataMember]
		public DatasourceV1[] Datasources { get; set; }

		[DataMember]
		public string CopyMetadata { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public string Subject { get; set; }

		[DataMember]
		public string Keywords { get; set; }

		[DataMember]
		public string Locale { get; set; }

		[DataMember]
		public string Hyphenate { get; set; }

		[DataMember]
		public bool TrackImports { get; set; }

		[DataMember]
		public int Timeout { get; set; }

		[DataMember]
		public bool RemoveUnusedFormats { get; set; }

		/// <summary>
		/// Enable or disable the error handling and verify functionality.
		/// Available options are:
		/// 0 - error handling and verify is disabled.  This is the default.
		/// 1 - enable error handling.
		/// 2 - enable verify.
		/// 3 - enable both error handling and verify.
		/// Any other value is ignored and disables the functionality.
		/// </summary>
		[DataMember]
		public int TrackErrors
		{
			get { return trackErrors; }
			set
			{
				trackErrors = value;
				// Correction
				if (trackErrors < 0 || trackErrors > 3)
					trackErrors = 0;
			}
		}

		private int trackErrors = 0;

		//**************************************************
		//
		// Printer specific options
		//
		//**************************************************

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

		/// <summary>
		/// A license key to be used for just this report. Overrides any license saved to the system.
		/// </summary>
		[DataMember]
		public string License { get; set; }
	}

}
