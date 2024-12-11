using Microsoft.AspNetCore.Mvc;
using PhoneShopeClient.Models;
using Stripe.Checkout;

namespace PhoneShopServer.Repositories
{
    public interface IPayment
    {
        string GeneratePaymentUrl(List<CartItem> cartItems, int orderId, string domain);
        Task<Session> GetSessionDetailsAsync(string sessionId);
    }
    public class PayementService: IPayment
    {
        public  string GeneratePaymentUrl(List<CartItem> cartItems, int orderId, string domain)
        {
            if (cartItems == null) return null;
            var lineItems = new List<SessionLineItemOptions>();
            cartItems.ForEach(ci => lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal= ci.Price * 100,
                    Currency    =   "eur",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = ci.Name,
                        Description = "garantie de remboursement de 1 mois"
                    }
                },
                Quantity = ci.Quantity,
            }));
            var sessionService = new SessionService();
            var sessionCreateOptions = new SessionCreateOptions
            {
                ClientReferenceId = orderId.ToString(),
                LineItems = lineItems,
                PaymentMethodTypes = ["card"],
                Mode = "payment",
                SuccessUrl =$"{domain}/paymment-success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl   = $"{domain}/payment-cancel",
            };
         
            var session = sessionService.Create(sessionCreateOptions);
            return session.Url ;
        }

        public async Task<Session> GetSessionDetailsAsync(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);
            return session;
        }
    }
}
