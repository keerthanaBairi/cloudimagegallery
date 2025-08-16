using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace ImageGalleryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _containerName = "images";
        private readonly string _connectionString;

        public ImagesController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("AzureBlobStorage")
                ?? throw new InvalidOperationException("AzureBlobStorage connection string is missing.");
        }

        // GET: api/images
        [HttpGet]
        public async Task<IActionResult> GetImages()
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            var imageUrls = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                imageUrls.Add(blobClient.Uri.ToString());
            }

            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Ok(imageUrls);
        }

        // GET: api/images/view/{fileName}
        [HttpGet("view/{fileName}")]
        public async Task<IActionResult> ViewImage(string fileName)
        {
            var containerClient = new BlobContainerClient(_connectionString, _containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
                return NotFound();

            var downloadInfo = await blobClient.DownloadContentAsync();
            var contentType = downloadInfo.Value.Details.ContentType ?? "image/jpeg";

            Response.Headers["Content-Disposition"] = "inline";
            Response.Headers.Add("Access-Control-Allow-Origin", "*");

            return File(downloadInfo.Value.Content.ToStream(), contentType);
        }
        [HttpDelete("{fileName}")]
public async Task<IActionResult> DeleteImage(string fileName)
{
    var containerClient = new BlobContainerClient(_configuration["StorageConnectionString"], "images");
    var blobClient = containerClient.GetBlobClient(fileName);

    if (await blobClient.ExistsAsync())
    {
        await blobClient.DeleteAsync();
        return NoContent();
    }

    return NotFound();
}

        // POST: api/images/upload-multiple
        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultiple(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                return BadRequest("No files uploaded.");
            }

            var uploadedUrls = new List<string>();
            var containerClient = new BlobContainerClient(_connectionString, _containerName);
            await containerClient.CreateIfNotExistsAsync();

            foreach (var file in files)
            {
                var blobClient = containerClient.GetBlobClient(file.FileName);

                var contentType = string.IsNullOrEmpty(file.ContentType) || file.ContentType == "application/octet-stream"
                    ? GetMimeType(file.FileName)
                    : file.ContentType;

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(
                        stream,
                        new BlobHttpHeaders { ContentType = contentType }
                    );
                }

                uploadedUrls.Add(blobClient.Uri.ToString());
            }

    
            Response.Headers.Append("Access-Control-Allow-Origin", "*");
            return Ok(uploadedUrls);
        }

        private string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => MediaTypeNames.Application.Octet
            };
        }
    }
}
