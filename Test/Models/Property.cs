using System.Runtime.Serialization;

namespace RESTfulEngine.Models
{
    /// <summary>
    /// Windward property.
    /// Properties are used to override the configuration file settings.
    /// </summary>
    [DataContract]
    public class Property
    {
		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public Property(string name, string value)
		{
			Name = name;
			Value = value;
		}

		/// <summary>
		/// Empty constructor. Need for (de)serialization.
		/// </summary>
		public Property()
		{
		}

		/// <summary>
        /// Name of the property.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Value of the property.
        /// </summary>
        [DataMember]
        public string Value { get; set; }
    }
}