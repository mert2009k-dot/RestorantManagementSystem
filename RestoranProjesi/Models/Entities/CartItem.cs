using System;

namespace RestoranProjesi.Models.Entities
{
    public class CartItem
    {
        public int Id { get; set; }
        
        public string CartId { get; set; } = string.Empty; // SessionId veya UserId
        
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
