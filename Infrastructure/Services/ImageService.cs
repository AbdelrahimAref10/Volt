using System;
using System.IO;
using System.Text;

namespace Infrastructure.Services
{
    public interface IImageService
    {
        string SaveBase64Image(string base64String, string folderName = "uploads");
        string? GetImageUrl(string fileName);
        bool DeleteImage(string fileName);
    }

    public class ImageService : IImageService
    {
        private readonly string _uploadsPath;
        private readonly string _baseUrl;

        public ImageService()
        {
            // For development, save to wwwroot/uploads
            _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            _baseUrl = "/uploads";

            // Ensure directory exists
            if (!Directory.Exists(_uploadsPath))
            {
                Directory.CreateDirectory(_uploadsPath);
            }
        }

        public string SaveBase64Image(string base64String, string folderName = "uploads")
        {
            if (string.IsNullOrWhiteSpace(base64String))
            {
                throw new ArgumentException("Base64 string cannot be empty", nameof(base64String));
            }

            // Remove data URL prefix if present (e.g., "data:image/png;base64,")
            var base64Data = base64String.Contains(",") 
                ? base64String.Split(',')[1] 
                : base64String;

            try
            {
                var imageBytes = Convert.FromBase64String(base64Data);
                
                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}.jpg";
                var folderPath = Path.Combine(_uploadsPath, folderName);
                
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);
                File.WriteAllBytes(filePath, imageBytes);

                // Return relative URL
                return $"{_baseUrl}/{folderName}/{fileName}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving image: {ex.Message}", ex);
            }
        }

        public string? GetImageUrl(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            // If already a full URL, return as is
            if (fileName.StartsWith("http://") || fileName.StartsWith("https://") || fileName.StartsWith("/"))
            {
                return fileName;
            }

            return $"{_baseUrl}/{fileName}";
        }

        public bool DeleteImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            try
            {
                // Extract filename from URL if needed
                var actualFileName = fileName.Contains("/") 
                    ? Path.GetFileName(fileName) 
                    : fileName;

                var filePath = Path.Combine(_uploadsPath, actualFileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}


