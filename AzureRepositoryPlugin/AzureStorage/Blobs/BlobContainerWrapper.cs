using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorage.Blobs
{
    internal class BlobContainerWrapper : IBlobContainerWrapper
    {
        private BlobServiceClient _serviceClient;

        private readonly string _storageConnectionString;
        private readonly string _containerPrefix;

        internal BlobContainerWrapper(string storageConnectionString, string containerPrefix)
        {
            _storageConnectionString = storageConnectionString;
            _containerPrefix = containerPrefix;
        }

        public async Task Init()
        {
            _serviceClient = new BlobServiceClient(_storageConnectionString);
        }

        private async Task<BlobContainerClient> GetBlobContainer(string containerName)
        {
            BlobContainerClient client;
            if (string.IsNullOrEmpty(_containerPrefix))
            {
                client = _serviceClient.GetBlobContainerClient($"{containerName}");
            } else
            {
                client = _serviceClient.GetBlobContainerClient($"{_containerPrefix}-{containerName}");
            }

            await client.CreateIfNotExistsAsync();

            return client;
        }

        private async Task<BlobClient> GetBlobClient(string containerName, string blobName)
        {
            BlobContainerClient client = await GetBlobContainer(containerName);
            BlobClient blob = client.GetBlobClient(blobName);

            return blob;
        }

        public async Task<Stream> GetStream(string containerName, string blobName)
        {
            BlobClient blob = await GetBlobClient(containerName, blobName);
            BlobDownloadInfo download = await blob.DownloadAsync();

            return download.Content;
        }

        public async Task<MemoryStream> GetInMemory(string containerName, string blobName)
        {
            Stream stream = await GetStream(containerName, blobName);

            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            return memoryStream;
        }

        public async Task<Response<BlobContentInfo>> UploadBase64(string containerName, string blobName, string base64)
        {
            BlobClient blob = await GetBlobClient(containerName, blobName);

            byte[] byteArray = Encoding.ASCII.GetBytes(base64);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                var response = await blob.UploadAsync(stream);
                return response;
            }
        }

        public async Task<Response<BlobContentInfo>> UploadBinaryData(string containerName, string blobName, byte[] data)
        {
            BlobClient blob = await GetBlobClient(containerName, blobName);

            using (MemoryStream stream = new MemoryStream(data))
            {
                var response = await blob.UploadAsync(stream);
                return response;
            }
        }

        public async Task<Response<bool>> DeleteBlob(string containerName, string blobName)
        {
            BlobClient blob = await GetBlobClient(containerName, blobName);

            var response = await blob.DeleteIfExistsAsync();
            return response;
        }

        public async Task<Response<bool>> DeleteContainer(string containerName)
        {
            BlobContainerClient client = await GetBlobContainer(containerName);

            var response = await client.DeleteIfExistsAsync();
            return response;
        }

        public async Task<string[]> GetAllBlobNamesInContainer(string containerName)
        {
            BlobContainerClient container = await GetBlobContainer(containerName);

            List<string> blobs = new List<string>();

            IAsyncEnumerator<BlobItem> iter = container.GetBlobsAsync().GetAsyncEnumerator();
            try
            {
                while (await iter.MoveNextAsync())
                {
                    BlobItem item = iter.Current;

                    blobs.Add(item.Name);
                }
            }
            finally { if (iter != null) await iter.DisposeAsync(); }

            return blobs.ToArray();
        }
        public async Task<Response<bool>> DoesBlobExist(string containerName, string blobName)
        {
            BlobClient blob = await GetBlobClient(containerName, blobName);
            return await blob.ExistsAsync();
        }
    }
}
