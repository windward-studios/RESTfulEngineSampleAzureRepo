using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorage.Tables
{
    public interface ICloudTableFactory
    {
        Task<ICloudTableWrapper<T>> CreateCloudTableWrapper<T>(string connectionString, string tableName) where T : ITableEntity, new();
    }
}
