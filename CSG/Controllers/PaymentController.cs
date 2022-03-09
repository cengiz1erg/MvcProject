using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSG.Services.Payment;

namespace WebItProject.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        //[Authorize]
        public IActionResult Index()
        
        
        {

            return View();
        }

        //[Authorize]
        [HttpPost]
        public IActionResult CheckInstallment(string binNumber)
        {
            if (binNumber.Length != 6)
                return BadRequest(new
                {
                    Message = "Bad req."
                });

            var result = _paymentService.CheckInstallments(binNumber, 90);
            return Ok(result);
        }
    }
}