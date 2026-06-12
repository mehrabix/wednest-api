using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ZarinPalService _zarinPal;
    private readonly IConfiguration _config;

    public PaymentsController(ZarinPalService zarinPal, IConfiguration config)
    {
        _zarinPal = zarinPal;
        _config = config;
    }

    /// <summary>
    /// Create a payment and get redirect URL to ZarinPal
    /// </summary>
    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        var result = await _zarinPal.CreatePaymentAsync(
            request.OrderId, request.Amount, request.Description,
            request.Email, request.Mobile);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// ZarinPal callback - user returns here after payment
    /// </summary>
    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string authority,
        [FromQuery] string Status,
        [FromQuery] Guid orderId)
    {
        if (Status != "OK")
            return Redirect($"{_config["FRONTEND_URL"] ?? "http://localhost:3000"}/payment/failed?orderId={orderId}");

        decimal amount = 0;
        using var scope = HttpContext.RequestServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WedNest.Infrastructure.Data.ApplicationDbContext>();
        var orderEntity = await context.Orders.FindAsync(orderId);
        if (orderEntity != null) amount = orderEntity.TotalAmount;

        var result = await _zarinPal.VerifyPaymentAsync(authority, amount, orderId);

        if (result.Success)
            return Redirect($"{_config["FRONTEND_URL"] ?? "http://localhost:3000"}/payment/success?orderId={orderId}&refId={result.RefId}");
        else
            return Redirect($"{_config["FRONTEND_URL"] ?? "http://localhost:3000"}/payment/failed?orderId={orderId}");
    }

    /// <summary>
    /// Verify payment manually (for API-based verification)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyRequest request)
    {
        var result = await _zarinPal.VerifyPaymentAsync(request.Authority, request.Amount, request.OrderId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

public class CheckoutRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "Wedding Registry Payment";
    public string Email { get; set; } = "";
    public string Mobile { get; set; } = "";
}

public class VerifyRequest
{
    public string Authority { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid OrderId { get; set; }
}
