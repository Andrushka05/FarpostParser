using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarpostParse
{
    public class Product
    {
        //<product>
        //<store_id>...</store_id>
        //<store_name>...</store_name>
        //<name>...</name>
        //<description>...</description>
        //<price>...</price>
        //<preview>...<preview>
        //<photos>
        //<photo>photo1</photo>
        //<photo>photo2</photo>
        //<photo>photo3</photo>
        //</photos>
        //</product>
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string Preview { get; set; }
        public List<string> Photos { get; set; }
    }
}
