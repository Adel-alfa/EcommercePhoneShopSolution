using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoneShopSharedLibrary.Models;
using PhoneShopServer.Repositories;

namespace PhoneShopServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategory CategoryReposirory) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAllCategory()
        {
            var category = await CategoryReposirory.GetAllCategories();
            return Ok(category);    
        }
        [HttpPost]
        public async Task<ActionResult> AddCategory(Category model)
        {
            if(model is null) return BadRequest("Model is null");
            var response = await CategoryReposirory.AddCategory(model);
            return Ok(response);    
        }
    }
}
