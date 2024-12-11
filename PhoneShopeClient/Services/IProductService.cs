using PhoneShopSharedLibrary.Models;
using PhoneShopSharedLibrary.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneShopeClient.Services
{
    public interface IProductService
    {
        Task<ServiceResponse> AddProduct(Product model);
        Task GetAllProducts(bool featuredProducts);
        Task GetProductsByCategoryId(int categoryId);
        Action? ProductAction { get; set; }
        List<Product> AllProducts { get; set; }
        List<Product>  FeaturedProducts { get; set; }
        List<Product>  ProductsByCategory { get; set; }
        Product GetRandomProduct();
        bool IsVisable { get; set; }
}
}
