
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSystem.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        // Foreign keys
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        // Snapshot vid köptillfället
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        // Navigation
        public Order Order { get; set; }
        public Product Product { get; set; }

    }
}
