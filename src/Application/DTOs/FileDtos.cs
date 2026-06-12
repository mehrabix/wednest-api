namespace WedNest.Application.DTOs;

public class UploadResult
{
    public bool Success { get; set; }
    public string? FileName { get; set; }
    public string? Url { get; set; }
    public string? Message { get; set; }
}
