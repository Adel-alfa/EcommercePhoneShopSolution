using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneShopSharedLibrary.Models
{
    public class OrderItem
    {
        public int Id { get; set; }       
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public string? Description { get; set; } = String.Empty;
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
