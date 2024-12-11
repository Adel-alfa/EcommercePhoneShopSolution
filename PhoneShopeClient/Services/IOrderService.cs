using PhoneShopeClient.Models;
using PhoneShopSharedLibrary.DTOs;
using PhoneShopSharedLibrary.Models;
using PhoneShopSharedLibrary.Responses;
using Stripe.Checkout;

namespace PhoneShopeClient.Services
{
    public interface IOrderService
    {

        Task<int> AddOrder(List<CartItem> carts, string UserEmail,string IpAddress);
        Task<ServiceResponse> UpdateOrder(Order model);
        Task GetAllOrders(string Role, string? Email);
        Task<string> OrderPayment(List<CartItem> cartItems, int orderId, string domain);
        Task<Session> UpdateOrderPaymentStatusAsync(string sessionId);
        Action? OrderAction { get; set; }
        Task<List<OrderDetailsModel>> GetOrder(int Id);
        List<Order> AllOrders { get; set; }
        List<Order> UserAllOrders { get; set; }
        bool IsOrderVisable { get; set; }
    }
}
