using Microsoft.AspNetCore.Http;

namespace Cinema.Services.IServices
{
    public interface IImageService
    {
        Task<string?> UploadAsync(IFormFile? file, string folder, string? oldFile = null);
        Task<string?> UploadMultipleAsync(List<IFormFile>? files, string folder, string? oldFiles = null);
        Task DeleteAsync(string? fileName, string folder);
        Task DeleteMultipleAsync(string? fileNames, string folder);
        string GetFullPath(string folder, string? fileName);
        List<string> GetMultiplePaths(string folder, string? fileNames);
    }
}
