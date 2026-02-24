using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using DefaultNamespace;
namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public PaymentController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }
        [Authorize]
        [HttpPost("create-checkout/{userId}")]
        public async Task<IActionResult> CreateCheckout(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "eur",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Vibelink Premium",
                                Description = "Swipes ilimitados"
                            },
                            UnitAmount = 999, // 9.99 EUR en centimos
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = "http://localhost:5093/api/payment/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "http://localhost:5093/api/payment/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() }
                }
            };
            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return Ok(new { url = session.Url });
        }
        [HttpGet("success")]
        public async Task<IActionResult> Success([FromQuery] string session_id)
        {
            var service = new SessionService();
            var session = await service.GetAsync(session_id);
            if (session.PaymentStatus == "paid")
            {
                var userId = int.Parse(session.Metadata["userId"]);
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsPremium = true;
                    await _context.SaveChangesAsync();
                    return Ok("¡Felicidades! Ahora eres premium.");
                }
            }
            return BadRequest("Error en el pago");
        }
        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            return Ok("Pago cancelado");
        }
    }
}