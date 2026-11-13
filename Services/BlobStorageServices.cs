
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        private const long MaxFileSize = 5 * 1024 * 1024;

        public BlobStorageService(
            BlobServiceClient blobServiceClient,
            IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }

        public async Task<string> UploadImageAsync(
            IFormFile file,
            string containerName)
        {
            // STEP 1: Check that a file was provided.
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException(
                    "Please select an image.");
            }

            // STEP 2: Limit images to 5 MB.
            if (file.Length > MaxFileSize)
            {
                throw new ArgumentException(
                    "Image must not exceed 5 MB.");
            }

            // STEP 3: Validate the file extension.
            string extension = Path.GetExtension(
                file.FileName
            ).ToLowerInvariant();

            string[] allowedExtensions =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    "Only JPG, PNG and WebP images are allowed.");
            }

            // STEP 4: Verify the actual image signature.
            byte[] header = new byte[12];

            using (var stream = file.OpenReadStream())
            {
                int bytesRead = await stream.ReadAsync(
                    header.AsMemory(0, header.Length));

                bool isJpeg =
                    bytesRead >= 3 &&
                    header[0] == 0xFF &&
                    header[1] == 0xD8 &&
                    header[2] == 0xFF;

                bool isPng =
                    bytesRead >= 8 &&
                    header.AsSpan(0, 8).SequenceEqual(
                        new byte[]
                        {
                            137, 80, 78, 71, 13, 10, 26, 10
                        });

                bool isWebp =
                    bytesRead >= 12 &&
                    System.Text.Encoding.ASCII.GetString(
                        header, 0, 4) == "RIFF" &&
                    System.Text.Encoding.ASCII.GetString(
                        header, 8, 4) == "WEBP";

                bool validSignature = extension switch
                {
                    ".jpg" or ".jpeg" => isJpeg,
                    ".png" => isPng,
                    ".webp" => isWebp,
                    _ => false
                };

                if (!validSignature)
                {
                    throw new ArgumentException(
                        "The selected file is not a valid supported image.");
                }
            }

            // STEP 5: Generate a unique filename.
            string fileName =
                $"{Guid.NewGuid():N}{extension}";

            // STEP 6: Access the existing Azure container.
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(
                    containerName
                );

            var blobClient =
                containerClient.GetBlobClient(fileName);

            // STEP 7: Assign the correct content type.
            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            // STEP 8: Upload to Azure.
            using var uploadStream = file.OpenReadStream();

            await blobClient.UploadAsync(
                uploadStream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                });

            // STEP 9: Return the blob URL.
            return blobClient.Uri.ToString();
        }


        public async Task<(Stream Content, string ContentType)> DownloadImageAsync(
            string imageUrl,
            string containerName)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                throw new ArgumentException("Image URL is required.");
            }

            // STEP 1: Locate the configured Azure container.
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(containerName);

            // STEP 2: Parse the stored blob URL.
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var imageUri))
            {
                throw new ArgumentException("Invalid image URL.");
            }

            // STEP 3: Verify that the URL belongs to our container.
            var containerUri = containerClient.Uri;

            if (!string.Equals(
                    imageUri.Scheme,
                    Uri.UriSchemeHttps,
                    StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(
                    imageUri.Host,
                    containerUri.Host,
                    StringComparison.OrdinalIgnoreCase) ||
                imageUri.Port != containerUri.Port)
            {
                throw new ArgumentException(
                    "Image URL does not belong to the configured storage account.");
            }

            string containerPath = containerUri.AbsolutePath.TrimEnd('/') + "/";

            if (!imageUri.AbsolutePath.StartsWith(
                    containerPath,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Image URL does not belong to the configured container.");
            }

            // STEP 4: Extract the blob name.
            string blobName = Uri.UnescapeDataString(
                imageUri.AbsolutePath[containerPath.Length..]);

            if (string.IsNullOrWhiteSpace(blobName) ||
                blobName.Contains('/') ||
                blobName.Contains('\\'))
            {
                throw new ArgumentException("Invalid blob name.");
            }

            // STEP 5: Retrieve the blob using our authenticated Azure client.
            var blobClient = containerClient.GetBlobClient(blobName);

            var download = await blobClient.DownloadStreamingAsync();

            string contentType =
                download.Value.Details.ContentType ?? "application/octet-stream";

            return (download.Value.Content, contentType);
        }
    }
}
