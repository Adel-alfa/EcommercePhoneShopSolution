using Microsoft.AspNetCore.Mvc;
using PhoneShopSharedLibrary.DTOs;
using PhoneShopSharedLibrary.Models;
using PhoneShopSharedLibrary.Responses;

namespace PhoneShopServer.Repositories
{
    public interface IOrder
    {

        Task<int> AddOrder(Order model);
        Task<ServiceResponse> UpdateOrder(Order model);//change OrderStatus => shipping, refunded  .....
        Task<List<Order>> GetAllOrders(string? userEmail);

        Task<List<OrderDetailsModel>> GetOrderDetails(int id);
    }
}
