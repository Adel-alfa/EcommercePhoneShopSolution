using Microsoft.EntityFrameworkCore;
using PhoneShopSharedLibrary.Models;
using PhoneShopSharedLibrary.Responses;
using PhoneShopServer.Data;


namespace PhoneShopServer.Repositories
{
    public class ProductRepository : IProduct
    {
        private readonly AppDbContext appDbContext;

        public ProductRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }
        public async Task<ServiceResponse> AddProduct(Product model)
        {
            if(model == null)  return new ServiceResponse(false,"Model is null!"); 
            var (flag,message) = await CheckName(model.Name!);
            if(flag)
            {
                appDbContext.Products.Add(model);
                await Commit();
                return new ServiceResponse(true, "Produit enregistré");
                    
            }
            return new ServiceResponse(false, message);
        }

        public async Task<List<Product>> GetAllProducts(bool featuredProducts)
        {
            if(featuredProducts)
                return await appDbContext.Products.Where(_=> _.Featured).ToListAsync();
            else 
               return await appDbContext.Products.ToListAsync();
        }

        private async Task<ServiceResponse> CheckName(string name)
        {
            var product = await appDbContext.Products.FirstOrDefaultAsync(_=> _.Name!.ToLower().Equals(name.ToLower()));
            return product is null ? new ServiceResponse(true, null!) : new ServiceResponse(false, "Le produit existe déjà");
        }
        private async Task Commit() => await appDbContext.SaveChangesAsync();
    }
}
