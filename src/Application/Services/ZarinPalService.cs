using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WedNest.Domain.Entities;
using WedNest.Infrastructure.Data;

namespace WedNest.Application.Services;

public class ZarinPalService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;

    public ZarinPalService(HttpClient http, IConfiguration config, ApplicationDbContext context)
    {
        _http = http;
        _config = config;
        _context = context;
    }

    private bool IsSandbox => _config["ZARINPAL_SANDBOX"]?.ToLower() == "true";
    private string BaseUrl => IsSandbox ? "https://sandbox.zarinpal.com" : "https://api.zarinpal.com";
    private string PaymentUrl => $"{BaseUrl}/pg/v4/payment/request.json";
    private string VerifyUrl => $"{BaseUrl}/pg/v4/payment/verify.json";
    private string RedirectUrl => IsSandbox ? "https://sandbox.zarinpal.com/pg/StartPay" : "https://www.zarinpal.com/pg/StartPay";
    private string MerchantId => _config["ZARINPAL_MERCHANT_ID"] ?? "";
    private string CallbackUrl => _config["ZARINPAL_CALLBACK_URL"] ?? "";

    public async Task<PaymentRequestResult> CreatePaymentAsync(Guid orderId, decimal amount, string description, string email, string mobile)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return new PaymentRequestResult { Success = false, Message = "Order not found" };

        var request = new
        {
            merchant_id = MerchantId,
            amount = (int)amount,
            callback_url = $"{CallbackUrl}?orderId={orderId}",
            description = description,
            metadata = new { email, mobile }
        };

        var response = await _http.PostAsJsonAsync(PaymentUrl, request);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var code = json.GetProperty("code").GetInt32();
        if (code != 100)
            return new PaymentRequestResult { Success = false, Message = $"ZarinPal error: {json.GetProperty("message").GetString()}" };

        var authority = json.GetProperty("data").GetProperty("authority").GetString()!;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            StripePaymentIntentId = authority,
            StripeSessionId = authority,
            Amount = amount,
            Currency = "IRR",
            Status = PaymentStatus.Pending,
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return new PaymentRequestResult
        {
            Success = true,
            Authority = authority,
            PaymentUrl = $"{RedirectUrl}/{authority}"
        };
    }

    public async Task<PaymentVerifyResult> VerifyPaymentAsync(string authority, decimal amount, Guid orderId)
    {
        var request = new
        {
            merchant_id = MerchantId,
            amount = (int)amount,
            authority
        };

        var response = await _http.PostAsJsonAsync(VerifyUrl, request);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var code = json.GetProperty("code").GetInt32();
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == authority);

        if (payment != null)
        {
            payment.UpdatedAt = DateTime.UtcNow;

            if (code == 100 || code == 101)
            {
                var refId = json.GetProperty("data").GetProperty("ref_id").GetString()!;
                payment.Status = PaymentStatus.Succeeded;
                payment.PaidAt = DateTime.UtcNow;
                payment.StripeSessionId = refId;

                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Completed;
                    order.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return new PaymentVerifyResult { Success = true, RefId = refId, Message = "Payment verified" };
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = $"ZarinPal code: {code}";
                await _context.SaveChangesAsync();
            }
        }

        return new PaymentVerifyResult { Success = false, Message = $"Payment failed with code: {code}" };
    }
}

public class PaymentRequestResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Authority { get; set; }
    public string? PaymentUrl { get; set; }
}

public class PaymentVerifyResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? RefId { get; set; }
}
