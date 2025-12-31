using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PhotoCoop.Domain.Payments;
using System.Linq;

namespace PhotoCoop.Infrastructure.Razorpay;

public class RazorpayClient : IRazorpayClient
{
    private readonly HttpClient _http;
    private readonly RazorpayOptions _opt;
    private readonly AuthenticationHeaderValue _authHeader;

    public RazorpayClient(HttpClient http, Microsoft.Extensions.Options.IOptions<RazorpayOptions> opt)
    {
        _http = http;
        _opt = opt.Value;

        _opt.KeyId = FirstNonEmpty(_opt.KeyId, Environment.GetEnvironmentVariable("Razorpay__KeyId"), Environment.GetEnvironmentVariable("RAZORPAY_KEY_ID"))!;
        _opt.KeySecret = FirstNonEmpty(_opt.KeySecret, Environment.GetEnvironmentVariable("Razorpay__KeySecret"), Environment.GetEnvironmentVariable("RAZORPAY_KEY_SECRET"))!;

        if (string.IsNullOrWhiteSpace(_opt.KeyId) || string.IsNullOrWhiteSpace(_opt.KeySecret))
            throw new InvalidOperationException("Razorpay credentials are missing. Set Razorpay:KeyId and Razorpay:KeySecret (env Razorpay__KeyId/Razorpay__KeySecret).");

        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_opt.KeyId}:{_opt.KeySecret}"));
        _authHeader = new AuthenticationHeaderValue("Basic", auth);

        _http.DefaultRequestHeaders.Authorization = _authHeader;
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _http.BaseAddress = new Uri("https://api.razorpay.com/");
    }

    public async Task<RazorpayOrderCreateResponse> CreateOrderAsync(RazorpayOrderCreateRequest request, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(request);
        using var msg = new HttpRequestMessage(HttpMethod.Post, "v1/orders")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        msg.Headers.Authorization = _authHeader;

        var resp = await _http.SendAsync(msg, ct);
        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<RazorpayOrderCreateResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async Task<RazorpayRefundResponse> RefundPaymentAsync(string paymentId, RazorpayRefundRequest request, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(request);
        using var msg = new HttpRequestMessage(HttpMethod.Post, $"v1/payments/{paymentId}/refund")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        msg.Headers.Authorization = _authHeader;

        var resp = await _http.SendAsync(msg, ct);
        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<RazorpayRefundResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
