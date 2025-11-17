using Cinema.Services.IServices;
using Microsoft.AspNetCore.Hosting;

namespace Cinema.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string?> UploadAsync(IFormFile? file, string folder, string? oldFile = null)
        {
            if (file == null || file.Length == 0)
                return oldFile; 

            if (!string.IsNullOrEmpty(oldFile))
                await DeleteAsync(oldFile, folder);

            var uploadPath = Path.Combine(_env.WebRootPath, "images", folder);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            return fileName;
        }

        public async Task<string?> UploadMultipleAsync(List<IFormFile>? files, string folder, string? oldFiles = null)
        {
            if (files == null || files.Count == 0)
                return oldFiles; 

            if (!string.IsNullOrEmpty(oldFiles))
                await DeleteMultipleAsync(oldFiles, folder);

            var uploadedNames = new List<string>();
            foreach (var file in files)
            {
                var fileName = await UploadAsync(file, folder);
                if (!string.IsNullOrEmpty(fileName))
                    uploadedNames.Add(fileName);
            }

            return string.Join(',', uploadedNames);
        }

        public async Task DeleteAsync(string? fileName, string folder)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var path = Path.Combine(_env.WebRootPath, "images", folder, fileName);
            if (File.Exists(path))
                await Task.Run(() => File.Delete(path));
        }

        public async Task DeleteMultipleAsync(string? fileNames, string folder)
        {
            if (string.IsNullOrEmpty(fileNames)) return;

            var files = fileNames.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var file in files)
                await DeleteAsync(file.Trim(), folder);
        }

        public string GetFullPath(string folder, string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            return Path.Combine("/images", folder, fileName).Replace("\\", "/");
        }

        public List<string> GetMultiplePaths(string folder, string? fileNames)
        {
            if (string.IsNullOrEmpty(fileNames))
                return new List<string>();

            return fileNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => GetFullPath(folder, f.Trim()))
                .ToList();
        }
    }
}
