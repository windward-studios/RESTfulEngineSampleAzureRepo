/*
* Copyright (c) 2015 by Windward Studios, Inc. All rights reserved.
*
* This software is the confidential and proprietary information of
* Windward Studios ("Confidential Information").  You shall not
* disclose such Confidential Information and shall use it only in
* accordance with the terms of the license agreement you entered into
* with Windward Studios, Inc.
*/


using System;
using System.Runtime.Serialization;

namespace RESTfulEngine.Models
{
    /// <summary>
    /// An exception that occured attempting to execute a request.
    /// </summary>
    [DataContract]
    public class ServiceError
    {
        /// <summary>
        /// Empty constructor. Need for (de)serialization.
        /// </summary>
		public ServiceError()
		{
		}

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public ServiceError(Exception ex)
		{
			Message = ex.Message;
			Type = ex.GetType().FullName;
			if (ex.InnerException != null)
				InnerError = new ServiceError(ex.InnerException);
		}

		/// <summary>
        /// The exception message.
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// The exception type.
        /// </summary>
		[DataMember]
		public string Type { get; set; }

        /// <summary>
        /// If there's an inner exception, the InnerException's Message.
        /// </summary>
        [DataMember]
        public ServiceError InnerError { get; set; }

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return $"Message: {Message}, Type: {Type}" + (InnerError == null ? "" : $", Inner: {InnerError}");
		}
	}
}