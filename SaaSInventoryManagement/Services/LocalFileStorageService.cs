using SaaSInventoryManagement.Exceptions;
using SaaSInventoryManagement.Services.Interfaces_;

namespace SaaSInventoryManagement.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private const string UploadsRootFolder = "uploads";

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder, FileUploadOptions? options = null)
        {
            options ??= new FileUploadOptions();

            if (file is null || file.Length == 0)
                throw new BadRequestException("No file was uploaded.");

            if (file.Length > options.MaxSizeBytes)
                throw new BadRequestException(
                    $"File is too large. Maximum allowed size is {options.MaxSizeBytes / (1024 * 1024)} MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !options.AllowedExtensions.Contains(extension))
                throw new BadRequestException(
                    $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", options.AllowedExtensions)}.");

            var safeFolder = string.Join("_", folder.Split(Path.GetInvalidFileNameChars().Concat(new[] { '/', '\\' }).ToArray()));

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var targetDirectory = Path.Combine(webRoot, UploadsRootFolder, safeFolder);
            Directory.CreateDirectory(targetDirectory);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(targetDirectory, fileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/{UploadsRootFolder}/{safeFolder}/{fileName}";
            _logger.LogInformation("Saved upload to {RelativePath}", relativePath);
            return relativePath;
        }

        public void DeleteFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return;

            try
            {
                var webRoot = _env.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                var trimmed = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
                var physicalPath = Path.Combine(webRoot, trimmed);

                var fullWebRoot = Path.GetFullPath(webRoot);
                var fullTarget = Path.GetFullPath(physicalPath);
                if (!fullTarget.StartsWith(fullWebRoot, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Refused to delete file outside wwwroot: {RelativePath}", relativePath);
                    return;
                }

                if (File.Exists(fullTarget))
                    File.Delete(fullTarget);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file {RelativePath}", relativePath);
            }
        }
    }
}
