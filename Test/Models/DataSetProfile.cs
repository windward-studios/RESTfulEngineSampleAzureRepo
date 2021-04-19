using System.Runtime.Serialization;

namespace RESTfulEngine.Models
{
	/// <summary>
	/// A dataset in a datasource.
	/// </summary>
	[DataContract]
	public class DataSetProfile
	{
		/// <summary>
		/// The name of this dataset.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The select of this dataset.
		/// </summary>
		public string Select { get; }

		/// <summary>
		/// All the properties for this dataset.
		/// </summary>
		[DataMember]
		public Entry[] Properties { get; set; }

		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public DataSetProfile()
		{
			Properties = new Entry[0];
		}
	}
}