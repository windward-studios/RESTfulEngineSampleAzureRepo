using System.Runtime.Serialization;

namespace RESTfulEngine.Models
{
	/// <summary>
	/// The generated tag tree we are sending back to the client.
	/// </summary>
	[DataContract]
	public class TagTree {
		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public TagTree(Template template, byte[] xml)
		{
			Guid = template.Guid;
			Tag = template.Tag;
			Xml = xml;
		}

		public TagTree()
		{
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

		/// <summary>
		/// The tag tree which is an XML document.
		/// </summary>
		[DataMember]
		public byte[] Xml { get; set; }
	}
}