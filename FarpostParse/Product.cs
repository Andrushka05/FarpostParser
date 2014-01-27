using System.Collections.Generic;

namespace FarpostParse
{
    public class Product
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public List<string> Photos { get; set; }
    }
}
