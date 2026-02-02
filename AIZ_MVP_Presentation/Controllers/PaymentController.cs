using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.RequestDtos;
using AIZ_MVP_Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Wrappers;
using System.Security.Claims;

namespace AIZ_MVP_Presentation.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        // Giả sử bạn có Service xử lý logic
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// API để User lấy mã QR thanh toán
        /// </summary>
        [Authorize]
        [HttpPost("checkout")]
        public async Task<IActionResult> CreateCheckout([FromBody] decimal price)
        {
            var identity = HttpContext.GetUserIdentity();
            if (identity == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Error = new ApiError { Code = "UNAUTHORIZED", Message = "User identity not found" }
                });
            }

            var result = await _paymentService.CreatePaymentRequest(identity, price);

            return Ok(new ApiResponse<object>
            {
                Data = result
            });
        }

        /// <summary>
        /// API Webhook dành riêng cho SePay
        /// Cấu hình URL này trên Dashboard của SePay: https://your-domain.com/api/payment/sepay-webhook
        /// </summary>
        [HttpPost("sepay-webhook")]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookModel data)
        {
            if (data == null || string.IsNullOrEmpty(data.content))
                return BadRequest(new { status = "failed", message = "Invalid data" });

            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.Contains("AIZ_Zenith_SP2026"))
            {
                return Unauthorized();
            }
            var isSuccess = await _paymentService.ProcessWebhook(data.content, data.transferAmount);

            if (isSuccess)
            {
                return Ok(new { status = "success" });
            }
            return BadRequest(new { status = "failed", message = "Transaction not found or invalid" });
        }
    }
}
