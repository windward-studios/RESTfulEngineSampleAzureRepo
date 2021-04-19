using System.Runtime.Serialization;

namespace RESTfulEngine.Models
{
	/// <summary>
	/// A parameter passed to the engine to be referenced using a ${var}.
	/// </summary>
	[DataContract]
	public class Parameter
	{
		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public Parameter(string name, object value)
		{
			Name = name;
			WrappedValue = new ParamValue(value);
		}

		/// <summary>For the ASP.NET constructor for controllers.</summary>
		public Parameter()
		{
		}

		/// <summary>
		/// Name of the parameter (the var in ${var}).
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// The Value of the parameter.
		/// </summary>
		[DataMember]
		public ParamValue WrappedValue { get; set; }
	}
}