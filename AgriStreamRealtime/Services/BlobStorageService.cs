using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace AgriStreamRealtime.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        // Constructor to initialize the BlobStorageService with the Azure Storage connection string from configuration.
        public BlobStorageService(IConfiguration configuration)
        {
            string? connectionString =
                configuration["AzureStorage:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
            }

            // Create a BlobServiceClient using the connection string and get a reference to the "game-covers" container.
            var blobServiceClient =
                new BlobServiceClient(connectionString);

            _containerClient =
                blobServiceClient.GetBlobContainerClient("game-covers");
        }
        public string GetReadOnlyImageUrl(string blobUrl)
        {
            Uri uri = new Uri(blobUrl);

            string blobName = Path.GetFileName(uri.LocalPath);

            BlobClient blobClient =
                _containerClient.GetBlobClient(blobName);

            if (!blobClient.CanGenerateSasUri)
            {
                throw new InvalidOperationException(
                    "Unable to generate image access link.");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
        public async Task<string> UploadImageAsync(IFormFile image)
        {
            // Validate the uploaded image file for null, size, and allowed extensions.
            if (image == null || image.Length == 0)
            {
                throw new ArgumentException("Please select an image.");
            }

            // Validate the uploaded image file for null, size, and allowed extensions.
            if (image.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException(
                    "Image size cannot exceed 5 MB.");
            }

            string extension =
                Path.GetExtension(image.FileName).ToLowerInvariant();

            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            // Create the container if it doesn't exist and set its access level to private.
            await _containerClient.CreateIfNotExistsAsync(
                PublicAccessType.None);

            string fileName = $"{Guid.NewGuid()}{extension}";

            BlobClient blobClient =
                _containerClient.GetBlobClient(fileName);

            await using Stream stream = image.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    // Set the content type of the uploaded blob to match the uploaded image's content type.
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = image.ContentType
                    }
                });

            return blobClient.Uri.ToString();
        }
    }
}
