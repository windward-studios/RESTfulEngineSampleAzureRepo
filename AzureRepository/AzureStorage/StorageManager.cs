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

        public AzureStorageManager GetAzureStorageManager()
        {
            return AzureStorageFactory.CreateTemplateStoragePlugin();
        }
    }
}
