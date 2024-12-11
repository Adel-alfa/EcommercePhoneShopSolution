
using Blazored.LocalStorage;
using PhoneShopeClient.Models;
using PhoneShopeClient.Services.Authentication;
using PhoneShopSharedLibrary.Models;
using PhoneShopSharedLibrary.Responses;
using System.Globalization;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Reflection.Metadata;
using static PhoneShopeClient.Services.ClientServices;
using System;
using System.Net.Http.Json;
using static System.Collections.Specialized.BitVector32;
using Stripe.Checkout;
using PhoneShopSharedLibrary.DTOs;

namespace PhoneShopeClient.Services
{
    public class ClientServices : IProductService, ICategoryService,ICart, IOrderService
    {
        private const string ProductBaseUrl = "api/product";
        private const string CategoryBaseUrl = "api/category";
        private const string PaymentBaseUrl = "api/payment";
        private const string OrderBaseUrl = "api/order";

        private readonly AuthenticationService authenticationService;
        private readonly HttpClient httpClient;
        private readonly ILocalStorageService localStorageService;
        public ClientServices(AuthenticationService _authenticationService,
            HttpClient _httpClient,
            ILocalStorageService _localStorageService)
        {
            authenticationService = _authenticationService;
            httpClient = _httpClient;
            localStorageService = _localStorageService;
        }


        public Action? ProductAction { get; set; }
        public List<Product> AllProducts { get; set; }
        public List<Product> FeaturedProducts { get; set; }
        public Action? CategoryAction { get; set; }
        public List<Category> AllCategories { get; set; }
        public List<Product> ProductsByCategory { get ; set; }
        public  bool IsVisable { get; set; }
        public Action? CartAction { get ; set ; }
        public int CartCount { get ; set ; }
        public bool IsCartLoaderVisible { get ; set ; }
        public Action? OrderAction { get ; set; }
        public List<Order> AllOrders { get; set ; }
        public List<Order> UserAllOrders { get ; set ; }
        public bool IsOrderVisable { get; set; }

        public async Task<ServiceResponse> AddProduct(Product model)
        {
            await authenticationService.GetUserDetailsAsync();
            var privateHttpClient = await authenticationService.AddHeaderToHttpClientAsync();
            if (privateHttpClient is null)
                return new ServiceResponse(false, "Unauthorized");
            var response = await httpClient.PostAsync(ProductBaseUrl, General.GenerateStringContent(General.SerializeObj(model)));

            if(!response.IsSuccessStatusCode) 
                return new ServiceResponse(false, "Une erreur s'est produite, Veuillez réessayer!");
            var apiResponse  = await response.Content.ReadAsStringAsync();            

            var data = General.DeserializeJsonString<ServiceResponse>(apiResponse);
            if (!data.flag) return data;
            await ReGetProducts();
            return data;
        }
        public async Task ReGetProducts()
        {
            bool featuredProduct = true;
            bool allProduct = false;
            AllProducts = null;
            FeaturedProducts = null;
            await GetAllProducts(featuredProduct);
            await GetAllProducts(allProduct);
        }
        public async Task GetAllProducts(bool featuredProducts)
        {
            if (featuredProducts && FeaturedProducts is null)
            {
                IsVisable = true;
                var response = await GetProductsFromApiAsync(featuredProducts);
                IsVisable= false;
                if (!response.IsSuccessStatusCode) return ;
                var result = await response.Content.ReadAsStringAsync();
                FeaturedProducts = (List<Product>?)General.DeszrializeJsonStringList<Product>(result)!;
                ProductAction?.Invoke();
                return;
            }
            else
            {
                if(!featuredProducts && AllProducts is null)
                {
                    IsVisable = true;
                    var response = await GetProductsFromApiAsync(featuredProducts);
                    IsVisable = false;
                    if (!response.IsSuccessStatusCode) return ;
                    var result = await response.Content.ReadAsStringAsync();
                    AllProducts = (List<Product>?)General.DeszrializeJsonStringList<Product>(result)!;
                    ProductAction?.Invoke();
                    return;
                }
            }
          
           
            
        }
        public async Task GetProductsByCategoryId(int categoryId)
        {
            bool featured = false;
            await GetAllProducts(featured);
            ProductsByCategory =  AllProducts.Where(p=>p.CategoryId == categoryId).ToList();
            ProductAction?.Invoke();
        }
        public Product GetRandomProduct()
        {
            if(FeaturedProducts is null) return null!;
            Random RandomNumbers = new();
            int minmumNumber = FeaturedProducts.Min(f=> f.Id);
            int maxmumNumber = FeaturedProducts.Max(f=> f.Id)+ 1;
            int result  = RandomNumbers.Next(minmumNumber, maxmumNumber);
            return FeaturedProducts.FirstOrDefault(f => f.Id == result)!;
        }
        private async Task<HttpResponseMessage> GetProductsFromApiAsync(bool featuredProducts)=>  await httpClient.GetAsync($"{ProductBaseUrl}?featured ={featuredProducts}");
        
        //Categories Servies
        public async Task<ServiceResponse> AddCategory(Category model)
        {
            await authenticationService.GetUserDetailsAsync();
            var privateHttpClient = await authenticationService.AddHeaderToHttpClientAsync();
            if (privateHttpClient is null) 
                return new ServiceResponse(false,"Unauthorized");
            var response = await httpClient.PostAsync(CategoryBaseUrl, General.GenerateStringContent(General.SerializeObj(model)));

            if (!response.IsSuccessStatusCode)
                return new ServiceResponse(false, "Une erreur s'est produite, Veuillez réessayer!");

            var apiResponse = await response.Content.ReadAsStringAsync();
           
            var data =  General.DeserializeJsonString<ServiceResponse>(apiResponse);
            if (!data.flag ) return data;
            await ReGetCategories();
            return data;
        }

        public async Task GetAllCategories()
        {
            if(AllCategories is null)
            {
                var response = await httpClient.GetAsync(CategoryBaseUrl);
                if (!response.IsSuccessStatusCode) return;
                var result = await response.Content.ReadAsStringAsync();
                AllCategories = (List<Category>?)General.DeszrializeJsonStringList<Category>(result)!;
                CategoryAction?.Invoke();
            }          
           
          
        }
        public async Task ReGetCategories()
        {           
            AllCategories = null;            
            await GetAllCategories();            
        }

        #region CartService
        public async Task GetCartCount()
        {
            string cartString = await GetCartFromLocalStorageAsync();
            if (string.IsNullOrEmpty(cartString))
                CartCount = 0;
            else
                CartCount = General.DeszrializeJsonStringList<StorageCart>(cartString).Count();
            CartAction?.Invoke();
        }

        public async Task<ServiceResponse> AddToCart(Product product, int quntity = 1)
        {
            string message = string.Empty;
            var myCart = new  List<StorageCart>();
            var getCartFromStorage =  await GetCartFromLocalStorageAsync();
            if (!string.IsNullOrEmpty(getCartFromStorage))
            {
                myCart = (List<StorageCart>)General.DeszrializeJsonStringList<StorageCart>(getCartFromStorage);
                var checkIfAddedAlready = myCart.FirstOrDefault(_ => _.ProductId == product.Id);
                if (checkIfAddedAlready is null)
                {
                    myCart.Add(new StorageCart() { ProductId = product.Id, Quantity = quntity });
                    message = "Produit Ajouté au Panier";
                }
                else
                {
                  
                    var newQuantity  = checkIfAddedAlready.Quantity + quntity;
                    
                    var UpdateProduct = new StorageCart() { ProductId = product.Id, Quantity = newQuantity };
                    myCart.Remove(checkIfAddedAlready);
                    if(newQuantity> 0)
                    myCart.Add(UpdateProduct);
                    message = "Produit mis à jour dans le panier";
                }
            }
            else
            {
                myCart.Add(new StorageCart() { ProductId = product.Id, Quantity = quntity });
                message = "Produit Ajouté au Panier";
            }
            await RemoveCartFromLocalStorageAsync();
            await SetCartFromLocalStorageAsync(General.SerializeObj(myCart));
            await GetCartCount();
            return new ServiceResponse(true, message);
        }

        public async Task<List<CartItem>> MyCartOrders()
        {
            IsCartLoaderVisible = true;
            var cartList = new List<CartItem>();
            string? myCartString = await GetCartFromLocalStorageAsync();
            if(string.IsNullOrEmpty(myCartString)) return null!;

            var myCartList =General.DeszrializeJsonStringList<StorageCart>(myCartString);
            await GetAllProducts(false);
            foreach (var cartItem in myCartList)
            {
                var product = AllProducts.FirstOrDefault(_=>_.Id == cartItem.ProductId);
                cartList.Add(new CartItem()
                {
                    Id = product.Id,
                    Name = product.Name,
                    Quantity = cartItem.Quantity,
                    Price = product.Price,
                    Image = product.Base64Img,
                });

            }
            IsCartLoaderVisible=false;
            await GetCartCount();
            return cartList;
        }
        public async Task<ServiceResponse> VideCart(List<CartItem> carts)
        {
            var myCartList = General.DeszrializeJsonStringList<StorageCart>(await GetCartFromLocalStorageAsync()).ToList();
            if (myCartList is null)
                return new ServiceResponse(false, "Produit non trouvé");
            myCartList.Clear();
            await RemoveCartFromLocalStorageAsync();
            await SetCartFromLocalStorageAsync(General.SerializeObj(myCartList));
            await GetCartCount();
            return new ServiceResponse(true, "le panier est vide");
        }
        public async Task<ServiceResponse> DeleteCart(CartItem cart)
        {
            var myCartList = General.DeszrializeJsonStringList<StorageCart>(await GetCartFromLocalStorageAsync()).ToList();
            if (myCartList is null)
                return new ServiceResponse(false, "Produit non trouvé");
            myCartList.Remove(myCartList.FirstOrDefault(_=>_.ProductId == cart.Id)!);
            await RemoveCartFromLocalStorageAsync();
            await SetCartFromLocalStorageAsync(General.SerializeObj(myCartList));
            await GetCartCount();
            return new ServiceResponse(true, "Produit retiré du panier");
        }
        public async Task<string> OrderPayment(List<CartItem> cartItems, int orderId, string domain)
        {
         
            var paymentRequest = new PaymentRequest { Parameter1 = cartItems, Parameter2 = orderId, Parameter3 = domain };
            var jsonContent = JsonSerializer.Serialize(paymentRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("api/order/checkout", content);
            var url  = await response.Content.ReadAsStringAsync();
            return url;
        }
        public async Task<Session> UpdateOrderPaymentStatusAsync(string sessionId)
        {

            var response = await httpClient.GetAsync($"api/Order/Update-OrderStatus/{sessionId}");
            if (!response.IsSuccessStatusCode) 
            {
                throw new Exception("Impossible de récupérer les détails de la session."); 
            }
            var json = await response.Content.ReadAsStringAsync(); 
            return JsonSerializer.Deserialize<Session>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }


        private async Task<string?> GetCartFromLocalStorageAsync() => await localStorageService.GetItemAsStringAsync("cart");
        private async Task SetCartFromLocalStorageAsync(string cart) => await localStorageService.SetItemAsStringAsync("cart",cart);
        private async Task RemoveCartFromLocalStorageAsync() => await localStorageService.RemoveItemAsync("cart");

       
        public class PaymentRequest
        {
            public List<CartItem> Parameter1 { get; set; }
            public int Parameter2 { get; set; }
            public string Parameter3 { get; set; }
        }

        #endregion

        #region OrderService
         
        public async Task<int> AddOrder(List<CartItem> carts, string UserEmail, string IpAddress)
        {
            if (carts == null) throw new ArgumentNullException(nameof(carts));


             Order myOrder = new();
            myOrder.UserEmail = UserEmail;
            myOrder.IpAddress = IpAddress;
            myOrder.OrderStatus = "Created";
            foreach (var item in carts)
            {
                var itm = new OrderItem
                {
                    UnitPrice = item.Price,
                    Quantity = item.Quantity,
                    ProductId = item.Id,
                };
                myOrder.OrderItems.Add(itm);
            }          

            await authenticationService.GetUserDetailsAsync();
            var privateHttpClient = await authenticationService.AddHeaderToHttpClientAsync();
            if (privateHttpClient is null)
                return 0; /*new ServiceResponse(false, "Unauthorized")*/;

          
            var jsonContent = JsonSerializer.Serialize(myOrder);
            Console.WriteLine(jsonContent);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
           
            var response = await httpClient.PostAsync(OrderBaseUrl, content);

            if (!response.IsSuccessStatusCode)
                return 0;/*new ServiceResponse(false, "Une erreur s'est produite, Veuillez réessayer!");*/

            var apiResponse = await response.Content.ReadAsStringAsync();

            var orderId = General.DeserializeJsonString<int>(apiResponse);
            if (orderId<=0) return 0;

            await ReGetOrders(UserEmail);



            return orderId;
        }
        public async Task ReGetOrders(string userEmail)
        {
            AllOrders = null;
            UserAllOrders = null;
           
          
            await GetAllOrders("Admin");
            await GetAllOrders("User",userEmail);
        }
        public Task<ServiceResponse> UpdateOrder(Order model)
        {
            throw new NotImplementedException();
        }

        public async Task GetAllOrders(string Role, string? userEmail =null)
        {
            if (Role == "User"  && UserAllOrders is null)
            {
                IsOrderVisable = true;
                var response = await GetOrderFromApiAsync(userEmail);
                IsOrderVisable = false;
                if (!response.IsSuccessStatusCode) return;
                var result = await response.Content.ReadAsStringAsync();
                UserAllOrders = (List<Order>?)General.DeszrializeJsonStringList<Order>(result)!;
                OrderAction?.Invoke();
                return;
            }
            else
            {
                if (Role == "Admin"  && AllOrders is null)
                {
                    IsOrderVisable = true;
                    var response = await GetOrderFromApiAsync(Role);
                    IsOrderVisable  =false;
                    if (!response.IsSuccessStatusCode) return;
                    var result = await response.Content.ReadAsStringAsync();
                   
                    Console.WriteLine(result);
                 
                    AllOrders = (List<Order>?)General.DeszrializeJsonStringList<Order>(result)!;
                  
                    OrderAction?.Invoke();
                    return;
                }
            }
        }

        public async Task<List<OrderDetailsModel>> GetOrder(int Id)
        {
            var response = await httpClient.GetAsync($"{OrderBaseUrl}/Details/{Id}");
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
            var order = General.DeszrializeJsonStringList<OrderDetailsModel>(result);  
            Console.WriteLine(result);
            return order.ToList();

        }
        private async Task<HttpResponseMessage> GetOrderFromApiAsync(string userEmail) =>
         await httpClient.GetAsync($"{OrderBaseUrl}/{userEmail}");
            
       

      


        #endregion

    }
}
