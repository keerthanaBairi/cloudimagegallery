using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageGalleryWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=contosogallery546;AccountKey=5PPTDyVWHZx7GPtjebpONGJ2LVCyIn87gGQviEwwvZQZPLMtsiQOn7zCnCc9fqQIICJkN2MavJ+S+AStDvMnHA==;EndpointSuffix=core.windows.net";
        private readonly string _containerName = "images";

        public List<string> ImageUrls { get; set; } = new();

        [BindProperty]
        public IFormFile? UploadedFile { get; set; }

        public async Task OnGetAsync()
        {
            await LoadImagesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UploadedFile != null)
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobClient = containerClient.GetBlobClient(UploadedFile.FileName);

                using (var stream = UploadedFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                await blobClient.DeleteIfExistsAsync();
            }

            return RedirectToPage();
        }

        private async Task LoadImagesAsync()
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            ImageUrls.Clear();

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                ImageUrls.Add(blobClient.Uri.ToString());
            }
        }
    }
}
