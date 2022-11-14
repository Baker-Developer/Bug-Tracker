using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BugTrackerImageService : IImageService
    {
        public string ContentType(IFormFile file)
        {
            return file?.ContentType;
        }

        public string DecodeImage(byte[] data, string type)
        {
            if (data is null || type is null)
            {
                return null;
            }
            return $"data:image/{type};base64,{Convert.ToBase64String(data)}";
        }

        public async Task<byte[]> EncodeImageAsync(IFormFile file)
        {
            if (file is null)
            {
                return null;
            }
            using var memoryString = new MemoryStream();
            await file.CopyToAsync(memoryString);
            return memoryString.ToArray();
        }

        public async Task<byte[]> EncodeImageAsync(string fileName)
        {
            var file = $"{Directory.GetCurrentDirectory()}/wwwroot/img/{fileName}";

            return await File.ReadAllBytesAsync(file);
        }

        public int Size(IFormFile file)
        {
            return Convert.ToInt32(file?.Length);
        }
    }
}
