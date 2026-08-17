namespace SaaSInventoryManagement.Services.Interfaces_
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder, FileUploadOptions? options = null);

        void DeleteFile(string? relativePath);
    }

    public class FileUploadOptions
    {
        public static readonly string[] DefaultImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public const long DefaultMaxSizeBytes = 5 * 1024 * 1024; 

        public string[] AllowedExtensions { get; init; } = DefaultImageExtensions;
        public long MaxSizeBytes { get; init; } = DefaultMaxSizeBytes;
    }
}
