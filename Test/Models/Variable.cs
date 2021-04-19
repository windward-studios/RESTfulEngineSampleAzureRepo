/*
* Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
*
* This software is the confidential and proprietary information of
* Windward Studios ("Confidential Information").  You shall not
* disclose such Confidential Information and shall use it only in
* accordance with the terms of the license agreement you entered into
* with Windward Studios, Inc.
*/

//using net.windward.api.csharp;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RESTfulEngine.Models
{
    /// <summary>
    /// A var in a template that must be defined before running it.
    /// </summary>
    [DataContract]
    public class Variable
    {
		/// <summary>
		/// The name of the variable.
		/// </summary>
        [DataMember]
        public string Name { get; set; }

		/// <summary>
		/// The description of this variable. This is optional.
		/// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// The default value of this variable.
        /// </summary>
        [DataMember]
        public string Value { get; set; }

		/// <summary>
		/// What type of data this variable is.
		/// </summary>
        [DataMember]
        public string Type { get; set; }

		/// <summary>
		/// If this is from a UDT it optionally has the GUID of the UDT item.
		/// </summary>
        [DataMember]
        public string UdtGuid { get; set; }

		/// <summary>
		/// true if this variable must be set.
		/// </summary>
        [DataMember]
        public bool Required { get; set; }

		/// <summary>
		/// Can return all values for this select variable.
		/// </summary>
        [DataMember]
        public bool AllowAll { get; set; }

		/// <summary>
		/// Can return a list of values for this select variable.
		/// </summary>
        [DataMember]
        public bool AllowList { get; set; }

		/// <summary>
		/// Can set a filter for values for this select variable.
		/// </summary>
        [DataMember]
        public bool AllowFilter { get; set; }

		/// <summary>
		/// Can sort values for this select variable.
		/// </summary>
        [DataMember]
        public bool AllowSort { get; set; }

		/// <summary>
		/// If this is an auto-select, this is the metadata for the select. For SQL this is [dbo.]table.column using the raw values (no surrounding spaces) and 
		/// for XML it is the full XPath to that node.
		/// </summary>
        [DataMember]
        public string AutoMetadata { get; set; }

        /// <summary>
        /// The default values for this var. Can be length 0 (which means no default).
        /// </summary>
        [DataMember]
        public VariableValue[] DefaultValues { get; set; }

		/// <summary>
		/// The allowed values for this var. Can be length 0 (which means anything is allowed OR the list was too long).
		/// </summary>
        [DataMember]
        public VariableValue[] AllowedValues { get; set; }

		/// <summary>
		/// The name of the datasource for the select.
		/// </summary>
        [DataMember]
        public string Datasource { get; set; }

		/// <summary>
		/// The date default value can be set to a calendar offset.
		/// </summary>
        [DataMember]
        public string CalOffset { get; set; }

		/// <summary>
		/// The Select for this var if a select var. null if not a select var.
		/// </summary>
		[DataMember]
		public string Select { get; set; }

		/// <summary>
		/// The Select format for this var if a select var. null if not a select var.
		/// </summary>
		[DataMember]
		public string SelectFormat { get; set; }

        /// <summary>
        /// Create from the engine TemplateVariable
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
   //     public static Variable Create(TemplateVariable var)
   //     {
   //         Variable v = new Variable();

   //         v.Name = var.Name;
   //         v.Description = var.Description;
   //         v.Type = TypeToString(var.VarType);
   //         v.UdtGuid = var.UdtGuid;
   //         v.Required = var.Required;
   //         v.AllowAll = var.AllowAll;
   //         v.AllowList = var.AllowList;
   //         v.AllowFilter = var.AllowFilter;
   //         v.AllowSort = var.AllowSort;
   //         v.AutoMetadata = var.AutoMetadata;
   //         v.DefaultValues = MakeValuesArray(var.DefaultValues);
   //         v.AllowedValues = MakeValuesArray(var.AllowedValues);
   //         v.Datasource = var.Datasource;
   //         v.CalOffset = CalOffsetToString(var.CalOffset);
			//v.Select = var.Select;
			//v.SelectFormat = var.SelectFormat;

   //         return v;
   //     }

   //     private static string TypeToString(TemplateVariable.VAR_TYPE type)
   //     {
   //         switch (type)
   //         {
   //             case TemplateVariable.VAR_TYPE.AUTO_SELECT:
   //                 return "auto_select";
   //             case TemplateVariable.VAR_TYPE.BOOLEAN:
   //                 return "boolean";
   //             case TemplateVariable.VAR_TYPE.CURRENCY:
   //                 return "currency";
   //             case TemplateVariable.VAR_TYPE.DATE:
   //                 return "datetime";
   //             case TemplateVariable.VAR_TYPE.FLOAT:
   //                 return "float";
   //             case TemplateVariable.VAR_TYPE.INTEGER:
   //                 return "int";
   //             case TemplateVariable.VAR_TYPE.SELECT:
   //                 return "select";
   //             case TemplateVariable.VAR_TYPE.TEXT:
   //                 return "text";
   //             default:
   //                 return "";
   //         }
   //     }

   //     private static string CalOffsetToString(TemplateVariable.CAL_OFFSET off)
   //     {
   //         switch (off)
   //         {
   //             case TemplateVariable.CAL_OFFSET.FIXED:
   //                 return "fixed";
   //             case TemplateVariable.CAL_OFFSET.START_OF_MONTH:
   //                 return "start_of_month";
   //             case TemplateVariable.CAL_OFFSET.START_OF_QUARTER:
   //                 return "start_of_quarter";
   //             case TemplateVariable.CAL_OFFSET.START_OF_WEEK:
   //                 return "start_of_week";
   //             case TemplateVariable.CAL_OFFSET.START_OF_YEAR:
   //                 return "start_of_year";
   //             case TemplateVariable.CAL_OFFSET.TODAY:
   //                 return "today";
   //             default:
   //                 return "";
   //         }
   //     }

   //     private static VariableValue[] MakeValuesArray(IList<TemplateVariableValue> vals)
   //     {
   //         List<VariableValue> allVals = new List<VariableValue>();

   //         foreach (var val in vals)
   //             allVals.Add(VariableValue.Create(val));

   //         return allVals.ToArray();
   //     }
    }
}
