using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WedNest.Application.DTOs;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[Authorize]
[ApiController]
[Route("api/weddings/{weddingId:guid}/translations")]
public class TranslationsController : ControllerBase
{
    private readonly TranslationService _service;

    public TranslationsController(TranslationService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<WeddingTranslationDto>>> GetAll(Guid weddingId)
    {
        return await _service.GetWeddingTranslationsAsync(weddingId);
    }

    [HttpGet("{languageId:guid}")]
    public async Task<ActionResult<WeddingTranslationDto>> Get(Guid weddingId, Guid languageId)
    {
        var result = await _service.GetWeddingTranslationAsync(weddingId, languageId);
        if (result == null) return NotFound();
        return result;
    }

    [HttpPut]
    public async Task<ActionResult<WeddingTranslationDto>> Upsert(Guid weddingId, [FromBody] CreateTranslationRequest request)
    {
        var result = await _service.UpsertWeddingTranslationAsync(weddingId, request);
        return Ok(result);
    }
}

[Authorize]
[ApiController]
[Route("api/giftitems/{giftItemId:guid}/translations")]
public class GiftItemTranslationsController : ControllerBase
{
    private readonly TranslationService _service;

    public GiftItemTranslationsController(TranslationService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<GiftItemTranslationDto>>> GetAll(Guid giftItemId)
    {
        return await _service.GetGiftItemTranslationsAsync(giftItemId);
    }

    [HttpPut]
    public async Task<ActionResult<GiftItemTranslationDto>> Upsert(Guid giftItemId, [FromBody] UpsertTranslationBody body)
    {
        var result = await _service.UpsertGiftItemTranslationAsync(giftItemId, body.LanguageId, body.Name, body.Description);
        return Ok(result);
    }
}

[Authorize]
[ApiController]
[Route("api/cashfunds/{cashFundId:guid}/translations")]
public class CashFundTranslationsController : ControllerBase
{
    private readonly TranslationService _service;

    public CashFundTranslationsController(TranslationService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<CashFundTranslationDto>>> GetAll(Guid cashFundId)
    {
        return await _service.GetCashFundTranslationsAsync(cashFundId);
    }

    [HttpPut]
    public async Task<ActionResult<CashFundTranslationDto>> Upsert(Guid cashFundId, [FromBody] UpsertTranslationBody body)
    {
        var result = await _service.UpsertCashFundTranslationAsync(cashFundId, body.LanguageId, body.Name, body.Description);
        return Ok(result);
    }
}

public class UpsertTranslationBody
{
    public Guid LanguageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
