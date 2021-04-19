using System.Runtime.Serialization;

namespace RESTfulEngine.Models
{
	/// <summary>
	/// A dataset that is part of a datasource. This is a member of this dataset's parent datasource.
	/// </summary>
	[DataContract]
	public class DataSet
	{
		/// <summary>
		/// The dataset name. Used as the datasource name when applying to a template.
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// The query that defines the dataset. This quesry is run against the parent's datasource.
		/// </summary>
		[DataMember]
		public string Query { get; set; }
	}
}