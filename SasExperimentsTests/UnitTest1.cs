using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using FluentAssertions;
using Xunit;

namespace SasExperimentsTests
{
    
    public class UnitTest1
    {
        private readonly string containerName = "<<container name>>";
        private readonly string accountName = "<<storage account name>>";

        private readonly string accountKey = "<<storage account key>>";
        
        [Fact]
        public async Task Can_Write_To_Container_Using_Sas()
        {
            var sharedKeyCredential = new StorageSharedKeyCredential(
                accountName,
                accountKey);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                Resource = "C",
                StartsOn = DateTimeOffset.Now,
                ExpiresOn = DateTimeOffset.Now.AddHours(24)
            };

            sasBuilder.SetPermissions(BlobAccountSasPermissions.Write);

            var blobSasQueryParameters = sasBuilder.ToSasQueryParameters(sharedKeyCredential);
            
            var containerClient = new BlobContainerClient(
                new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"),
                new AzureSasCredential(blobSasQueryParameters.ToString()));

                await containerClient.UploadBlobAsync(
                    $"{Guid.NewGuid():N}.txt", 
                    new MemoryStream(Encoding.UTF8.GetBytes("foobar")));
        }

        [Fact]
        public async Task Can_Write_To_Blob_Using_Sas()
        {
            var sharedKeyCredential = new StorageSharedKeyCredential(
                accountName,
                accountKey);

            var blobName = $"{Guid.NewGuid():N}.txt";

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "B",
                StartsOn = DateTimeOffset.Now,
                ExpiresOn = DateTimeOffset.Now.AddHours(24)
            };

            sasBuilder.SetPermissions(BlobAccountSasPermissions.Write);

            var blobSasQueryParameters = sasBuilder.ToSasQueryParameters(sharedKeyCredential);

            var containerClient = new BlobContainerClient(
                new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"),
                new AzureSasCredential(blobSasQueryParameters.ToString()));

            await containerClient.UploadBlobAsync(
                blobName,
                new MemoryStream(Encoding.UTF8.GetBytes("foobar")));
        }
    }
}
