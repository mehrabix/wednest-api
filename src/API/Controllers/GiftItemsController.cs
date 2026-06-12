using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WedNest.Application.DTOs;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[ApiController]
[Route("api/weddings/{weddingId:guid}/[controller]")]
public class GiftItemsController : ControllerBase
{
    private readonly GiftItemService _service;

    public GiftItemsController(GiftItemService service) => _service = service;

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid weddingId) => Ok(await _service.GetByWeddingAsync(weddingId));

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result != null ? Ok(result) : NotFound();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Guid weddingId, [FromBody] CreateGiftItemRequest request)
    {
        var result = await _service.CreateAsync(weddingId, request);
        return CreatedAtAction(nameof(GetById), new { weddingId, id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGiftItemRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result != null ? Ok(result) : NotFound();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("api/weddings/{weddingId:guid}/[controller]")]
public class CashFundsController : ControllerBase
{
    private readonly CashFundService _service;

    public CashFundsController(CashFundService service) => _service = service;

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid weddingId) => Ok(await _service.GetByWeddingAsync(weddingId));

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Guid weddingId, [FromBody] CreateCashFundRequest request)
    {
        var result = await _service.CreateAsync(weddingId, request);
        return CreatedAtAction(nameof(GetAll), new { weddingId }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCashFundRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result != null ? Ok(result) : NotFound();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
