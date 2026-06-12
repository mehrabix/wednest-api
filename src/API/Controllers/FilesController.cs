using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WedNest.Application.DTOs;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileStorage _fileStorage;

    public FilesController(IFileStorage fileStorage) => _fileStorage = fileStorage;

    [HttpPost("upload/{folder}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Upload(string folder, IFormFile file)
    {
        var allowedFolders = new[] { "covers", "gifts", "avatars" };
        if (!allowedFolders.Contains(folder.ToLower()))
            return BadRequest(new { message = "Invalid folder. Allowed: covers, gifts, avatars" });

        var result = await _fileStorage.UploadAsync(file, folder.ToLower());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete]
    public IActionResult Delete([FromQuery] string url)
    {
        return _fileStorage.DeleteFile(url) ? NoContent() : NotFound();
    }
}
