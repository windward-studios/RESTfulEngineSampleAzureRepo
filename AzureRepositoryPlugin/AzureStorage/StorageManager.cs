using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureRepositoryPlugin.AzureStorage
{
    public class StorageManager
    {
        private AzureStorageManagerFactory AzureStorageFactory;

        public StorageManager()
        {
            AzureStorageFactory = new AzureStorageManagerFactory();
        }

        public async Task<AzureStorageManager> GetAzureStorageManager()
        {
            return await AzureStorageFactory.CreateTemplateStoragePlugin();
        }
    }
}
