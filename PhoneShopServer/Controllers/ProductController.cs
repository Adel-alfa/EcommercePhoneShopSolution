using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoneShopSharedLibrary.Models;
using PhoneShopServer.Repositories;

namespace PhoneShopServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProduct productReposirory) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAllProduct(bool featured)
        {
            var products = await productReposirory.GetAllProducts(featured);
            return Ok(products);    
        }
        [HttpPost]
        public async Task<ActionResult> AddProduct(Product model)
        {
            if(model is null) return BadRequest("Model is null");
            var response = await productReposirory.AddProduct(model);
            return Ok(response);    
        }
    }
}
