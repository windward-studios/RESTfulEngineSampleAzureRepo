using System.Runtime.Serialization;

namespace RESTfulEngine.Models
{
	/// <summary>
	/// A key value pair used in the data returned from the TagTree and Metrics for properties.
	/// </summary>
    [DataContract]
    public class Entry
    {
		/// <summary>
		/// The key (name) of the property.
		/// </summary>
		/// <value>The key (name) of the property.</value>
        [DataMember]
        public string Key { get; set; }

		/// <summary>
		/// The value of the property.
		/// </summary>
		/// <value>The value of the property.</value>
        [DataMember]
        public string Value { get; set; }
    }
}