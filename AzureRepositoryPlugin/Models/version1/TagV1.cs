//using net.windward.tags;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RESTfulEngine.Models.version1
{
    [DataContract]
    public class TagV1
    {
        [DataMember]
        public int XmlType { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public string NodeName { get; set; }

        [DataMember]
        public bool HasNode { get; set; }

        [DataMember]
        public string Node { get; set; }

        [DataMember]
        public string Datasource { get; set; }

        [DataMember]
        public Entry[] Attributes { get; set; }

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public long Guid { get; set; }

        //public static TagV1 Create(BaseTag baseTag)
        //{
        //    TagV1 tag = new TagV1();

        //    tag.XmlType = baseTag.getXmlType();
        //    tag.Text = baseTag.safeToText();
        //    tag.NodeName = baseTag.tagNodeName();
        //    tag.HasNode = baseTag.hasNode();
        //    tag.Node = baseTag.getNode();
        //    tag.Datasource = baseTag.getDatasource();
        //    tag.Attributes = MakeAttributesArray(baseTag.getAttributes());
        //    tag.Level = baseTag.getLevel();
        //    tag.Type = baseTag.getType();
        //    tag.Guid = baseTag.getGuid();

        //    return tag;
        //}

        //private static Entry[] MakeAttributesArray(java.util.Map attrs)
        //{
        //    List<Entry> allAttrs = new List<Entry>();

        //    for (var it = attrs.entrySet().iterator(); it.hasNext();)
        //    {
        //        var entry = (java.util.Map.Entry)it.next();
        //        allAttrs.Add(new Entry() { Key = (string)entry.getKey(), Value = (string)entry.getValue() });
        //    }

        //    return UtilsV1.MakeArray(allAttrs);
        //}
    }
}
