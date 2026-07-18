using JewelryStore.Infrastructure.Services.Payment;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("request")]
        public async Task<IActionResult> RequestPayment(
            int amount = 1000,
            string description = "تست پرداخت",
            string mobile = null,
            string email = null)
        {
            var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/payment/verify";

            var result = await _paymentService.RequestPaymentAsync(
                amount,
                description,
                callbackUrl,
                mobile,
                email
            );

            return Ok(result);
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyPayment(string authority, int amount = 1000)
        {
            var result = await _paymentService.VerifyPaymentAsync(authority, amount);
            return Ok(result);
        }
    }
}
