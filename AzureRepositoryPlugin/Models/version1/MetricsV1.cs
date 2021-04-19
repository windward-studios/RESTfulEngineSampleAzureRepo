//using net.windward.api.csharp;
//using net.windward.tags;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
//using WindwardReport = net.windward.api.csharp.Report;

namespace RESTfulEngine.Models.version1
{
    [DataContract]
    public class TemplateDatasourceV1
    {
        [DataMember]
        public Entry[] Dictionary { get; set; }
    }

    [DataContract]
    public class MetricsV1
    {
        [DataMember]
        public string TemplateType { get; set; }

        [DataMember]
        public string[] Datasources { get; set; }

        [DataMember]
        public TagV1[] Tags { get; set; }

        [DataMember]
        public string[] Vars { get; set; }

        [DataMember]
        public string[] PodGuids { get; set; }

        [DataMember]
        public Variable[] TemplateVariables { get; set; }

        [DataMember]
        public TemplateDatasourceV1[] TemplateDatasources { get; set; }

        [DataMember]
        public string AutoTagData { get; set; }

        [DataMember]
        public string AutotagVersion { get; set; }

        [DataMember]
        public string AutotagXml { get; set; }

        //public static MetricsV1 Create(TemplateMetrics tm)
        //{
        //    MetricsV1 m = new MetricsV1();

        //    m.TemplateType = TemplateTypeToString(tm.TemplateType);
        //    m.Datasources = UtilsV1.MakeArray(tm.Datasources);
        //    m.Tags = MakeTagsArray(tm.Tags);
        //    m.Vars = UtilsV1.MakeArray(tm.Vars);
        //    m.PodGuids = UtilsV1.MakeArray(tm.PodGuids);
        //    m.TemplateVariables = MakeVariablesArray(tm.TemplateVariables);
        //    m.TemplateDatasources = MakeTemplateDatasourcesArray(tm.DataSourceProfiles);
        //    m.AutoTagData = tm.AutoTagData;
        //    m.AutotagVersion = tm.AutotagVersion;
        //    if (tm.AutotagXml != null)
        //        m.AutotagXml = tm.AutotagXml.InnerXml;

        //    return m;
        //}

        //private static string TemplateTypeToString(WindwardReport.TEMPLATE_TYPE type)
        //{
        //    switch (type)
        //    {
        //        case WindwardReport.TEMPLATE_TYPE.DOCX:
        //            return "docx";
        //        case WindwardReport.TEMPLATE_TYPE.HTML:
        //            return "html";
        //        case WindwardReport.TEMPLATE_TYPE.PPTX:
        //            return "pptx";
        //        case WindwardReport.TEMPLATE_TYPE.UNKNOWN:
        //            return "unknown";
        //        case WindwardReport.TEMPLATE_TYPE.XLSX:
        //            return "xlsx";
        //        default:
        //            return "";
        //    }
        //}

        //private static TagV1[] MakeTagsArray(BaseTag[][] tags)
        //{
        //    var allTags = new List<TagV1>();
        //    foreach (var tagsArray in tags)
        //    {
        //        foreach (var baseTag in tagsArray)
        //        {
        //            allTags.Add(TagV1.Create(baseTag));
        //        }
        //    }
        //    return UtilsV1.MakeArray(allTags);
        //}

        //private static Variable[] MakeVariablesArray(IList<TemplateVariable> vars)
        //{
        //    var allVars = new List<Variable>();

        //    foreach (var v in vars)
        //    {
        //        allVars.Add(Variable.Create(v));
        //    }

        //    return UtilsV1.MakeArray(allVars);
        //}

        //private static TemplateDatasourceV1[] MakeTemplateDatasourcesArray(List<TemplateMetrics.DataSourceProfile> datasources)
        //{
        //    var allEntries = new List<TemplateDatasourceV1>();

        //    foreach (var d in datasources)
        //    {
        //        var datasource = d.Properties;
        //        TemplateDatasourceV1 tds = new TemplateDatasourceV1();
        //        var entries = new List<Entry>();
        //        foreach (var entry in datasource)
        //        {
        //            entries.Add(new Entry() { Key = entry.Key, Value = entry.Value });
        //        }
        //        entries.Add(new Entry()
        //        {
        //            Key = "full-type",
        //            Value = d.VendorType
        //        });
        //        string simpleType = "";
        //        switch (d.VendorType)
        //        {
        //            case "AdoDataSourceInfo":
        //                simpleType = "sql";
        //                break;
        //            case "JsonDataSourceInfo":
        //                simpleType = "json";
        //                break;
        //            case "ODataSourceInfo":
        //                simpleType = "odata";
        //                break;
        //            case "XmlDataSourceInfo":
        //                simpleType = "xml";
        //                break;
        //            case "SaxonDataSourceInfo":
        //                simpleType = "xml2";
        //                break;
        //            case "SFDataSourceInfo":
        //                simpleType = "salesforce";
        //                break;
        //            default:
        //                break;
        //        }
        //        entries.Add(new Entry()
        //        {
        //            Key = "simple-type",
        //            Value = simpleType
        //        });
        //        entries.Add(new Entry()
        //        {
        //            Key = "datasource-name",
        //            Value = d.Name
        //        });
        //        tds.Dictionary = UtilsV1.MakeArray(entries);
        //        allEntries.Add(tds);
        //    }

        //    return UtilsV1.MakeArray(allEntries);
        //}
    }
}
