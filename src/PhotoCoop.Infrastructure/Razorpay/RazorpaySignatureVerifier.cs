using System.Security.Cryptography;
using System.Text;

namespace PhotoCoop.Infrastructure.Razorpay;

public static class RazorpaySignatureVerifier
{
    public static bool VerifyWebhook(string rawBody, string receivedSignature, string webhookSecret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(webhookSecret);
        var bodyBytes = Encoding.UTF8.GetBytes(rawBody);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(bodyBytes);
        var expected = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        return SlowEquals(expected, receivedSignature);
    }

    // constant-time compare
    private static bool SlowEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }
}
