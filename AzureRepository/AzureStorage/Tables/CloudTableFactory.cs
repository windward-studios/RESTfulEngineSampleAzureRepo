using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorage.Tables
{
    public class CloudTableFactory : ICloudTableFactory
    {
        public async Task<ICloudTableWrapper<T>> CreateCloudTableWrapper<T>(string connectionString, string tableName) where T : ITableEntity, new()
        {
            CloudTableWrapper<T> table = new CloudTableWrapper<T>(connectionString, tableName);
            await table.Init();
            return table;
        }
    }
}
