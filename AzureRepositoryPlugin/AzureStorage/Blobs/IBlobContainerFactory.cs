using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorage.Blobs
{
    public interface IBlobContainerFactory
    {
        Task<IBlobContainerWrapper> CreateBlobContainerWrapper(string storageConnectionString, string containerPrefix);
    }
}
