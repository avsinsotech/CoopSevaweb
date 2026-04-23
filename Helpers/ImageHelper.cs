using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace AVSBackend.Helpers
{
    public static class ImageHelper
    {
        private const int MaxDimension = 1600;

        public static byte[]? CompressImage(byte[]? imageBytes, int targetSizeInBytes = 512000)
        {
            if (imageBytes == null || imageBytes.Length == 0) return null;

            // If already below target size, skip processing
            if (imageBytes.Length <= targetSizeInBytes) return imageBytes;

            try
            {
                using var image = Image.Load(imageBytes);

                // 1. Smart Resize if image is too large
                if (image.Width > MaxDimension || image.Height > MaxDimension)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(MaxDimension, MaxDimension),
                        Mode = ResizeMode.Max
                    }));
                }

                // 2. Dynamic JPEG Compression
                using var ms = new MemoryStream();
                int quality = 80;
                
                image.SaveAsJpeg(ms, new JpegEncoder { Quality = quality });

                // If still too large, aggressive compression
                if (ms.Length > targetSizeInBytes)
                {
                    ms.SetLength(0);
                    quality = 65; // Lower quality to hit target
                    image.SaveAsJpeg(ms, new JpegEncoder { Quality = quality });
                }

                // If STILL too large, even more aggressive
                if (ms.Length > targetSizeInBytes)
                {
                    ms.SetLength(0);
                    quality = 50; 
                    image.SaveAsJpeg(ms, new JpegEncoder { Quality = quality });
                }

                return ms.ToArray();
            }
            catch
            {
                // Fallback to original if something goes wrong
                return imageBytes;
            }
        }
    }
}
