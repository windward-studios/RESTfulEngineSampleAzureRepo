using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage.Blobs;
using AzureStorage.Tables;

namespace AzureRepositoryPlugin
{
    public class AzureStorageManagerFactory
    {
        public async Task<AzureStorageManager> CreateTemplateStoragePlugin()
        {
            CloudTableFactory tableFactory = new CloudTableFactory();
            BlobContainerFactory blobFactory = new BlobContainerFactory();
            AzureStorageManager plugin = new AzureStorageManager(tableFactory, blobFactory);
            await plugin.Init();
            return plugin;
        }
    }
}
