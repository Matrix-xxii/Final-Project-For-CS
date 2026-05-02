using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace FoodOutlet.AppCode
{
    /// <summary>
    /// Image processing service for resizing and padding images
    /// </summary>
    public class ImageProcessingService
    {
        private readonly IWebHostEnvironment _env;
        private const int TargetSize = 300; // 300x300 pixels
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public ImageProcessingService(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Processes an uploaded image: resizes to fit within 300x300, adds white padding, and saves
        /// </summary>
        public async Task<string> ProcessAndSaveImageAsync(IFormFile imageFile, string uploadFolder)
        {
            // Validate file
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("No file was uploaded");

            if (imageFile.Length > MaxFileSize)
                throw new ArgumentException("File size exceeds 5MB limit");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
            
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Only JPG, PNG, and WEBP formats are allowed");

            try
            {
                // Create uploads folder if it doesn't exist
                var uploadPath = Path.Combine(_env.WebRootPath ?? "wwwroot", uploadFolder);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}.jpg"; // Always save as JPG for consistency
                var filePath = Path.Combine(uploadPath, fileName);

                // Process image: resize to fit with white padding
                using (var stream = imageFile.OpenReadStream())
                {
                    using (var image = await Image.LoadAsync(stream))
                    {
                        // Calculate dimensions to fit within 300x300 while maintaining aspect ratio
                        var (resizedWidth, resizedHeight) = CalculateFitDimensions(image.Width, image.Height, TargetSize);
                        
                        // Resize image to fit
                        image.Mutate(img =>
                            img.Resize(new ResizeOptions
                            {
                                Size = new Size(resizedWidth, resizedHeight),
                                Mode = ResizeMode.Max,
                                Sampler = KnownResamplers.Lanczos3
                            })
                        );

                        // Add white padding around the resized image
                        image.Mutate(img =>
                            img.Pad(
                                TargetSize,
                                TargetSize,
                                Color.White
                            )
                        );

                        // Save as optimized JPG
                        var jpegOptions = new JpegEncoder { Quality = 85 };
                        await image.SaveAsJpegAsync(filePath, jpegOptions);
                    }
                }

                // Return relative path for database storage
                return $"/{uploadFolder}/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Image processing failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Calculates dimensions to fit image within target size while maintaining aspect ratio
        /// </summary>
        private (int width, int height) CalculateFitDimensions(int originalWidth, int originalHeight, int targetSize)
        {
            double aspectRatio = (double)originalWidth / originalHeight;
            int newWidth, newHeight;

            if (originalWidth > originalHeight)
            {
                // Landscape orientation
                newWidth = targetSize;
                newHeight = (int)Math.Round(targetSize / aspectRatio);
            }
            else if (originalHeight > originalWidth)
            {
                // Portrait orientation
                newHeight = targetSize;
                newWidth = (int)Math.Round(targetSize * aspectRatio);
            }
            else
            {
                // Square image
                newWidth = targetSize;
                newHeight = targetSize;
            }

            return (newWidth, newHeight);
        }

        /// <summary>
        /// Deletes an image file from disk
        /// </summary>
        public bool DeleteImage(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return false;

            try
            {
                var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", relativePath.TrimStart('/'));
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }

            return false;
        }
    }
}
