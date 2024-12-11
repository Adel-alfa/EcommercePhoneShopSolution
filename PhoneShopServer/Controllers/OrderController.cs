using Microsoft.AspNetCore.Mvc;
using PhoneShopSharedLibrary.DTOs;
using PhoneShopSharedLibrary.Models;
using PhoneShopServer.Repositories;
using static PhoneShopeClient.Services.ClientServices;


namespace PhoneShopServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrder orderRepo, IPayment PayementService) : ControllerBase
    {

        [HttpPost]
        public async Task<ActionResult> AddOrder(Order model)
        {

            if (model is null) return BadRequest("Model is null");
            var response = await orderRepo.AddOrder(model);
            return Ok(response);
        }

        [HttpGet("{userEmail}")]
        public async Task<ActionResult<List<Product>>> GetAllOrder(string userEmail)
        {
            var products = await orderRepo.GetAllOrders(userEmail);
            return Ok(products);
        }
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            List<OrderDetailsModel> order = await orderRepo.GetOrderDetails(id);
        
            return Ok(order);
        }
        [HttpPost("checkout")]
        public async Task<ActionResult> CrateCheckoutSession(PaymentRequest request)
        {
            var url = PayementService.GeneratePaymentUrl(request.Parameter1, request.Parameter2, request.Parameter3);
            return Ok(url);
        }
        [HttpGet("Update-OrderStatus/{sessionId}")]
        public async Task<IActionResult> UpdateOrderStatusInSession(string sessionId)
        {
            var session = await PayementService.GetSessionDetailsAsync(sessionId);
            if (session == null)
            {
                return NotFound();
            }
            int num;
            if (int.TryParse(session.ClientReferenceId, out num))
            {
                Order order = new Order();
                order.Id = num;
                order.OrderStatus = session.PaymentStatus; 
                var res = await orderRepo.UpdateOrder(order);
            }

            return Ok();
        }
    }
}
