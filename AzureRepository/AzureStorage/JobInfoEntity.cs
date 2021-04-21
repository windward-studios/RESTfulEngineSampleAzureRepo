using Microsoft.WindowsAzure.Storage.Table;
using RESTfulEngine.DocumentRepository;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace AzureRepositoryPlugin.AzureStorage
{
    [DataContract]
    public class JobInfoEntity : TableEntity
    {
        [DataMember]
        public Guid JobId { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public int Status { get; set; }

        [DataMember]
        public DateTime CreationDate { get; set; }

        public static JobInfoEntity FromJobRequestData(JobRequestData data, string partitionKey)
        {
            return new JobInfoEntity
            {
                PartitionKey = partitionKey,
                RowKey = data.Template.Guid.ToString(),
                JobId = Guid.Parse(data.Template.Guid),
                CreationDate = data.CreationDate,
                Type = (int)data.RequestType
            };
        }
    }
}
