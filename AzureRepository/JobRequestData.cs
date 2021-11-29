using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using WindwardModels;
using WindwardRepository;

namespace AzureRepositoryPlugin
{
    public enum JobRequestAction
    {
        CREATE,
        UPDATE_STATUS,
        GET_STATUS,
        GET_DOCUMENT,
        DELETE
    }

    [DataContract]
    public class JobRequestData
    {
        [DataMember]
        public Template Template { get; set; }

        [DataMember]
        public RepositoryStatus.REQUEST_TYPE RequestType { get; set; }

        [DataMember]
        public JobRequestAction Action { get; set; }

        [DataMember]
        public DateTime CreationDate { get; set; }
    }
}
