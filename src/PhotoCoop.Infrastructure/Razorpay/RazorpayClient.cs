using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PhotoCoop.Domain.Payments;

namespace PhotoCoop.Infrastructure.Razorpay;

public class RazorpayClient : IRazorpayClient
{
    private readonly HttpClient _http;
    private readonly RazorpayOptions _opt;

    public RazorpayClient(HttpClient http, Microsoft.Extensions.Options.IOptions<RazorpayOptions> opt)
    {
        _http = http;
        _opt = opt.Value;

        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_opt.KeyId}:{_opt.KeySecret}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        _http.BaseAddress = new Uri("https://api.razorpay.com/");
    }

    public async Task<RazorpayOrderCreateResponse> CreateOrderAsync(RazorpayOrderCreateRequest request, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(request);
        var resp = await _http.PostAsync("v1/orders", new StringContent(json, Encoding.UTF8, "application/json"), ct);
        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<RazorpayOrderCreateResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async Task<RazorpayRefundResponse> RefundPaymentAsync(string paymentId, RazorpayRefundRequest request, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(request);
        var resp = await _http.PostAsync($"v1/payments/{paymentId}/refund", new StringContent(json, Encoding.UTF8, "application/json"), ct);
        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<RazorpayRefundResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
