/*
* Copyright (c) 2020 by Windward Studios, Inc. All rights reserved.
*
* This software is the confidential and proprietary information of
* Windward Studios ("Confidential Information").  You shall not
* disclose such Confidential Information and shall use it only in
* accordance with the terms of the license agreement you entered into
* with Windward Studios, Inc.
*/

using System;
using System.Runtime.Serialization;
//using java.math;
//using Kailua.net.windward.utils;
//using net.windward.util.datetime;

namespace RESTfulEngine.Models
{
	/// <summary>
	/// To (de)serialize a parameter value, both endpoints need to be able to handle it. Therefore we
	/// can't use the type object. Some objects just aren't serializable (like FileStream). Others don't
	/// have the same thing at each end (Java ZonedDateTime vs .Net DateTimeOffset).
	/// So we have to pass these as a string and type and then build up the object from that.
	///
	/// The deserialization sets the type and rw value but the engine never has reason to read those.
	/// The engine just needs the converted back object value.
	/// </summary>
	[DataContract]
	public class ParamValue
	{
		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public ParamValue()
		{
			// the Value {get} is called from external code - no idea where/why. So set this so
			// Value returns null.
			ParamType = "null";
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		/// <param name="varValue">The value for this parameter.</param>
		public ParamValue(object varValue)
		{
			if (varValue == null)
			{
				ParamType = "null";
				RawValue = null;
				return;
			}

			if (varValue is string strValue)
			{
				ParamType = "string";
				RawValue = strValue;
				return;
			}

			if (varValue is bool)
			{
				ParamType = "boolean";
				RawValue = varValue.ToString();
				return;
			}

			if (varValue is DateTimeOffset dto)
			{
				ParamType = "datetime";
				RawValue = dto.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
				return;
			}

			if (varValue is DateTime dt)
			{
				ParamType = "datetime";
				RawValue = dt.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
				return;
			}

			if (varValue is decimal)
			{
				ParamType = "decimal";
				RawValue = varValue.ToString();
				return;
			}

			if (varValue.IsNumeric())
			{
				ParamType = "number";
				RawValue = varValue.ToString();
				return;
			}

			//Trap.trap();
			throw new ArgumentException($"Unknown value Type {varValue.GetType().Name}");
		}

		/// <summary>
		/// What type the raw value needs to be converted back to. Note that Number will use the most
		/// sensible type, but will always use double instead of float if the value has a fractional
		/// component.
		///
		/// And this can't be an enum because it has to be passed as a string and C# doesn't do string enums.
		///
		/// Allowed values: null, string, boolean, number (long or double), decimal (BigDouble in Java), or datetime (WindwardDateTime)
		/// </summary>
		[DataMember]
		public string ParamType { get; set; }

		/// <summary>
		/// The parameter value as a string.
		/// </summary>
		[DataMember]
		public string RawValue { get; set; }

		/// <summary>
		/// Not a DataMember! This converts the passed parameter to the object of the expected type.
		/// </summary>
		public object Value
		{
			get
			{
				switch (ParamType)
				{
					case "null":
						return null;
					case "string":
						return RawValue;
					case "boolean":
						return Convert.ToBoolean(RawValue);
					case "number":
						if (!RawValue.Contains("."))
							return Convert.ToInt64(RawValue);
						return Convert.ToDouble(RawValue);
					//case "decimal":
					//	if (!RawValue.Contains("."))
					//		return new BigInteger(RawValue);
					//	return new BigDecimal(RawValue);
					//case "datetime":
					//	// LocalDateTime: 2003-10-30T22:09:17
					//	// ZonedDateTime: 2018-10-21T08:11:55-06:00[America/Denver] 
					//	// OffsetDateTime: 2003-10-30T22:09:17-08:00 or 2003-10-30T22:09:17Z
					//	// C# doesn't have a ZonedDateTime
					//	return WindwardDateTime.parse(RawValue);
				}

				throw new ArgumentException($"Unknown ParamType {ParamType}");
			}
		}
	}

	static class NumExtensions { 

	// Extension method, call for any object, eg "if (x.IsNumeric())..."
		public static bool IsNumeric(this object x) { return (x != null && IsNumeric(x.GetType())); }

		// Method where you know the type of the object
		public static bool IsNumeric(Type type) { return IsNumeric(type, Type.GetTypeCode(type)); }

		// Method where you know the type and the type code of the object
		public static bool IsNumeric(Type type, TypeCode typeCode) { return (typeCode == TypeCode.Decimal || (type.IsPrimitive && typeCode != TypeCode.Object && typeCode != TypeCode.Boolean && typeCode != TypeCode.Char)); }
	}
}