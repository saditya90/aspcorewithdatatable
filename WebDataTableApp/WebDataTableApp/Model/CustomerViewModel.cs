using System.Collections.Generic;

namespace WebDataTableApp.Models
{
    public class CustomerViewModel
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public string Email { get; internal set; }
        public string AccountNumber { get; internal set; }
        public string AccountName { get; internal set; }
        public string Avatar { get; internal set; }
        public string Country { get; set; }
        public List<ProductDetail> ProductDetails { get; internal set; } = new List<ProductDetail>();
    }

    public class ProductDetail
    {
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public string Department { get; internal set; }
        public string Price { get; internal set; }
    }
}
