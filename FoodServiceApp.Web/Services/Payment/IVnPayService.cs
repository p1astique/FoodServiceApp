using Microsoft.AspNetCore.Http;

namespace FoodServiceApp.Web.Services.Payment
{
    public record VnPayCreateRequest(int OrderId, long AmountVnd, string OrderInfo, string IpAddress, string? ReturnUrl = null);

    public record VnPayCallbackResult(bool IsValidSignature, bool IsSuccess, int OrderId, string TransactionId, long Amount);

    public interface IVnPayService
    {
        /// <summary>Tạo URL thanh toán để redirect người dùng sang cổng VNPay.</summary>
        string CreatePaymentUrl(VnPayCreateRequest request);

        /// <summary>Xác thực dữ liệu trả về từ VNPay (ReturnUrl hoặc IPN) bằng chữ ký HMAC-SHA512.</summary>
        VnPayCallbackResult ValidateCallback(IQueryCollection query);
    }
}
