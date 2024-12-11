using Microsoft.EntityFrameworkCore;
using PhoneShopSharedLibrary.Models;
using PhoneShopSharedLibrary.Responses;
using PhoneShopServer.Data;


namespace PhoneShopServer.Repositories
{
    public class CategoryRepository : ICategory
    {
        private readonly AppDbContext appDbContext;

        public CategoryRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }
        

        public async Task<ServiceResponse> AddCategory(Category model)
        {
            if (model == null) return new ServiceResponse(false, "Model is null!");
            var (flag, message) = await CheckName(model.Name!);
            if (flag)
            {
                appDbContext.Categories.Add(model);
                await Commit();
                return new ServiceResponse(true, "Catégorie enregistrée");

            }
            return new ServiceResponse(false, message);
        }

        public async Task<List<Category>> GetAllCategories()=> await appDbContext.Categories.ToListAsync();
      

        

        private async Task<ServiceResponse> CheckName(string name)
        {
            var Category = await appDbContext.Categories.FirstOrDefaultAsync(_=> _.Name!.ToLower().Equals(name.ToLower()));
            return Category is null ? new ServiceResponse(true, null!) : new ServiceResponse(false, "La catégorie existe déjà");
        }
        private async Task Commit() => await appDbContext.SaveChangesAsync();
    }
}
