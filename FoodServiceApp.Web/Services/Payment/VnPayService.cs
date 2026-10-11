using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using FoodServiceApp.Web.Services.Configs;

namespace FoodServiceApp.Web.Services.Payment
{
    public class VnPayService : IVnPayService
    {
        private readonly VnPaySettings _settings;

        public VnPayService(IOptions<VnPaySettings> options)
        {
            _settings = options.Value;
        }

        public string CreatePaymentUrl(VnPayCreateRequest request)
        {
            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = _settings.Version,
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = _settings.TmnCode,
                ["vnp_Amount"] = ((long)(request.AmountVnd * 100)).ToString(), // VNPay yêu cầu nhân 100
                ["vnp_CurrCode"] = _settings.CurrCode,
                ["vnp_TxnRef"] = request.OrderId.ToString(),
                ["vnp_OrderInfo"] = request.OrderInfo,
                ["vnp_OrderType"] = "other",
                ["vnp_Locale"] = _settings.Locale,
                ["vnp_ReturnUrl"] = request.ReturnUrl ?? _settings.ReturnUrl,
                ["vnp_IpAddr"] = request.IpAddress,
                ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
            };

            var (queryString, hashData) = BuildQuery(vnpParams);
            var secureHash = HmacSha512(_settings.HashSecret, hashData);

            return $"{_settings.BaseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        }

        public VnPayCallbackResult ValidateCallback(IQueryCollection query)
        {
            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
            string receivedHash = string.Empty;

            foreach (var kv in query)
            {
                if (kv.Key == "vnp_SecureHash" || kv.Key == "vnp_SecureHashType")
                {
                    if (kv.Key == "vnp_SecureHash") receivedHash = kv.Value!;
                    continue;
                }
                if (kv.Key.StartsWith("vnp_"))
                    vnpParams[kv.Key] = kv.Value!;
            }

            var (_, hashData) = BuildQuery(vnpParams);
            var computedHash = HmacSha512(_settings.HashSecret, hashData);

            var isValid = string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);
            var responseCode = vnpParams.GetValueOrDefault("vnp_ResponseCode", "");
            var orderId = int.Parse(vnpParams.GetValueOrDefault("vnp_TxnRef", "0"));
            var amount = long.Parse(vnpParams.GetValueOrDefault("vnp_Amount", "0")) / 100;
            var transactionId = vnpParams.GetValueOrDefault("vnp_TransactionNo", "");

            return new VnPayCallbackResult(
                IsValidSignature: isValid,
                IsSuccess: isValid && responseCode == "00",
                OrderId: orderId,
                TransactionId: transactionId,
                Amount: amount
            );
        }

        private static (string QueryString, string HashData) BuildQuery(SortedDictionary<string, string> vnpParams)
        {
            var sb = new StringBuilder();
            foreach (var kv in vnpParams)
            {
                if (string.IsNullOrEmpty(kv.Value)) continue;
                sb.Append(Uri.EscapeDataString(kv.Key)).Append('=').Append(Uri.EscapeDataString(kv.Value)).Append('&');
            }
            var full = sb.ToString().TrimEnd('&');
            return (full, full);
        }

        private static string HmacSha512(string key, string inputData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
