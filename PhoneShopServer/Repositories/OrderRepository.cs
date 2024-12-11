using Microsoft.EntityFrameworkCore;
using PhoneShopSharedLibrary.DTOs;
using PhoneShopSharedLibrary.Models;
using PhoneShopSharedLibrary.Responses;
using PhoneShopServer.Data;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

namespace PhoneShopServer.Repositories
{
    public class OrderRepository(AppDbContext appDbContext) : IOrder
    {
        public async Task<int> AddOrder(Order model)
        {
            if (model == null) return 0;
            var order = appDbContext.Orders.Add(model).Entity;
            await Commit();
           
            return order.Id;
        }

        public async Task<List<Order>> GetAllOrders(string? userEmail=null )
        {
             
           if (!string.IsNullOrEmpty(userEmail) && !userEmail.Contains("Admin")) 
               return await appDbContext.Orders.Where(_=>_.UserEmail == userEmail).ToListAsync();
            else 
                return await appDbContext.Orders.ToListAsync(); 
            
        }

       
        public async Task<List<OrderDetailsModel>> GetOrderDetails(int Id)
        {
            List<OrderDetailsModel> OrderDetail = new();
            var order =  await appDbContext.Orders.AsNoTracking().Where(x => x.Id == Id).Include(x=>x.OrderItems).FirstOrDefaultAsync();
            foreach (var item in order.OrderItems)
            {
                OrderDetailsModel model = new OrderDetailsModel()
                {
                    Id = item.Id,
                    Description = item.Description,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice,
                    product = appDbContext.Products.FirstOrDefault(p => p.Id == item.ProductId),
                };
                OrderDetail.Add(model);
            }
            return OrderDetail;
        }

        public async Task<ServiceResponse> UpdateOrder(Order model)
        {
            var order = await appDbContext.Orders.FirstOrDefaultAsync(_ => _.Id == model.Id);
            if (order is not null )
            {
                order.OrderStatus = model.OrderStatus;
                appDbContext.Entry(order).State = EntityState.Modified;
                appDbContext.SaveChanges();
                return new ServiceResponse(true, " status changed ");
            }
            return new ServiceResponse(false , " order not found ");

        }
        private async Task Commit() => await appDbContext.SaveChangesAsync();
    }
}
