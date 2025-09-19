
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSystem.Models
{
    public class Order // Klass som representerar en kundorder i systemet
    {
        public int Id { get; set; }

        public string CustomerName { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public List<OrderItem> Items { get; set; } = new();
    }
}
