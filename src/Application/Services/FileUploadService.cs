using Microsoft.AspNetCore.Http;
using WedNest.Application.DTOs;

namespace WedNest.Application.Services;

public interface IFileStorage
{
    Task<UploadResult> UploadAsync(IFormFile file, string folder);
    bool DeleteFile(string url);
    string GetContentRoot();
}

public class FileUploadService : IFileStorage
{
    private readonly string _contentRoot;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    public FileUploadService(string contentRoot) => _contentRoot = contentRoot;

    public async Task<UploadResult> UploadAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            return new UploadResult { Success = false, Message = "No file provided" };

        if (file.Length > MaxFileSize)
            return new UploadResult { Success = false, Message = "File too large (max 5MB)" };

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return new UploadResult { Success = false, Message = "Invalid file type" };

        var uploadDir = Path.Combine(_contentRoot, "uploads", folder);
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var url = $"/uploads/{folder}/{fileName}";
        return new UploadResult { Success = true, FileName = fileName, Url = url };
    }

    public bool DeleteFile(string url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        var filePath = Path.Combine(_contentRoot, url.TrimStart('/'));
        if (!File.Exists(filePath)) return false;
        File.Delete(filePath);
        return true;
    }

    public string GetContentRoot() => _contentRoot;
}
