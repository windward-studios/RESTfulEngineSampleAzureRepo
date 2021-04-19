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
    public interface IBlobContainerWrapper
    {
        Task Init();
        Task<Stream> GetStream(string containerName, string blobName);
        Task<MemoryStream> GetInMemory(string containerName, string blobName);
        Task<string[]> GetAllBlobNamesInContainer(string containerName);

        Task<Response<BlobContentInfo>> UploadBase64(string containerName, string blobName, string base64);
        Task<Response<BlobContentInfo>> UploadBinaryData(string containerName, string blobName, byte[] data);
        Task<Response<bool>> DeleteBlob(string containerName, string blobName);
        Task<Response<bool>> DeleteContainer(string containerName);

        Task<Response<bool>> DoesBlobExist(string containerName, string blobName);
    }
}
