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
        private readonly string connectionString = "DefaultEndpointsProtocol=https;AccountName=fmduswsto;AccountKey=sS4km2vg9xf6aNkbF/F5BSkSFadHP8kdDlj/eXRJoosjSlcfpa3hTP3iR4Z64PgNYbrMnAVS00ghGrss1+yXLA==;EndpointSuffix=core.windows.net";
        private readonly string containerName = "test";
        private readonly string accountName = "fmduswsto";

        private readonly string accountKey =
            "sS4km2vg9xf6aNkbF/F5BSkSFadHP8kdDlj/eXRJoosjSlcfpa3hTP3iR4Z64PgNYbrMnAVS00ghGrss1+yXLA==";

        private readonly BlobServiceClient blobServiceClient;

        public UnitTest1()
        {
            this.blobServiceClient = new BlobServiceClient(this.connectionString);
        }

        [Fact]
        public void Can_Create_Container_Sas()
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                Resource = "C",
                ExpiresOn = DateTimeOffset.Now.AddHours(2)
            };

            sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

            var sasUri = blobServiceClient
                .GetBlobContainerClient(containerName)
                .GenerateSasUri(sasBuilder)
                .ToString();

            sasUri.Contains(containerName).Should().BeTrue();

        }

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

            sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

            var blobSasQueryParameters = sasBuilder.ToSasQueryParameters(sharedKeyCredential);
            
            var containerClient = new BlobContainerClient(
                new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"),
                new AzureSasCredential(blobSasQueryParameters.ToString()));

                await containerClient.UploadBlobAsync(
                    "1.txt", 
                    new MemoryStream(Encoding.UTF8.GetBytes("foobar")));
        }

        [Fact]
        public void Can_Create_Blob_Sas()
        {
            var blobName = "1.txt";

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "B",
                ExpiresOn = DateTimeOffset.Now.AddHours(2)
            };

            sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

            var sasUri = blobServiceClient
                .GetBlobContainerClient(containerName)
                .GetBlobClient(blobName)
                .GenerateSasUri(sasBuilder)
                .ToString();

            sasUri.Contains(blobName).Should().BeTrue();
        }
    }
}
