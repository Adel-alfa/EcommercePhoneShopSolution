using PhoneShopSharedLibrary.Models;
using PhoneShopSharedLibrary.Responses;

namespace PhoneShopeClient.Services
{
    public interface ICategoryService
    {
        Task<ServiceResponse> AddCategory(Category model);
        Task GetAllCategories();
        Action? CategoryAction { get; set; }
        List<Category> AllCategories { get; set; }
    }
}
