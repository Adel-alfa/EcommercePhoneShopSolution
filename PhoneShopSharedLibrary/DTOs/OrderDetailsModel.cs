using PhoneShopSharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneShopSharedLibrary.DTOs
{
    public class OrderDetailsModel
    {

        public int Id { get; set; }       
        public string? Description { get; set; } = String.Empty;
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }      
        public Product? product { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
