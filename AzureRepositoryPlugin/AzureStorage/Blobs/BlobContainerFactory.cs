using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorage.Blobs
{
    public class BlobContainerFactory : IBlobContainerFactory
    {
        public async Task<IBlobContainerWrapper> CreateBlobContainerWrapper(string storageConnectionString, string containerPrefix)
        {
            BlobContainerWrapper wrapper = new BlobContainerWrapper(storageConnectionString, containerPrefix);
            await wrapper.Init();
            return wrapper;
        }
    }
}
