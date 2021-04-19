using net.windward.api.csharp;
using System.Runtime.Serialization;

namespace RESTfulEngine.Models
{
    /// <summary>
    /// A value in the TemplateVar. Used for default and allowed values. Also used for the value of a parameter.
    /// </summary>
    [DataContract]
    public class VariableValue
    {
		/// <summary>
		/// The display text for a value.
		/// </summary>
        [DataMember]
        public string Label { get; set; }

		/// <summary>
		/// The name of this value.
		/// </summary>
        [DataMember]
        public string Name { get; set; }

		/// <summary>
		/// The value.
		/// </summary>
        [DataMember]
        public ParamValue Value { get; set; }

		/// <summary>
		/// What the value is returning. Allowed values are: literal, param_value, or select.
		/// </summary>
        [DataMember]
        public string ValueReference { get; set; }

        /// <summary>
        /// Create from the engine TemplateVariableValue.
        /// </summary>
        /// <param name="val">the engine TemplateVariableValue.</param>
        /// <returns>The converted VariableValue.</returns>
        public static VariableValue Create(TemplateVariableValue val)
        {
            VariableValue v = new VariableValue();

            v.Label = val.Label;
            v.Name = val.Name;
            v.Value = new ParamValue(val.Value);
            v.ValueReference = ValueReferenceToString(val.ValueReference);

            return v;
        }

        private static string ValueReferenceToString(TemplateVariableValue.VALUE_REFERENCE valRef)
        {
            switch (valRef)
            {
                case TemplateVariableValue.VALUE_REFERENCE.LITERAL:
                    return "literal";
                case TemplateVariableValue.VALUE_REFERENCE.PARAM_VALUE:
                    return "param_value";
                case TemplateVariableValue.VALUE_REFERENCE.SELECT:
                    return "select";
                default:
                    return "";
            }
        }
    }
}