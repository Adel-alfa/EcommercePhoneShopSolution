using PhoneShopeClient.Models;
using PhoneShopeClient.Pages.Cart;
using PhoneShopSharedLibrary.Models;
using PhoneShopSharedLibrary.Responses;
using Stripe.Checkout;

namespace PhoneShopeClient.Services
{
    public interface ICart
    {
        public Action? CartAction { get; set; }
        public int CartCount { get; set; }
        Task GetCartCount();
        Task<ServiceResponse> AddToCart(Product product, int Quntity = 1);
        Task<List<CartItem>> MyCartOrders();
        Task<ServiceResponse> DeleteCart(CartItem cart);
        Task<ServiceResponse> VideCart(List<CartItem> carts);
        bool IsCartLoaderVisible {  get; set; }
        
    }
}
