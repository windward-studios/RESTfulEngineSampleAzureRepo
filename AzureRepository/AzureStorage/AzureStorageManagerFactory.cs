using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureRepositoryPlugin
{
    public class AzureStorageManagerFactory
    {
        public AzureStorageManager CreateTemplateStoragePlugin()
        {
            AzureStorageManager plugin = new AzureStorageManager();
            plugin.Init();
            return plugin;
        }
    }
}
