using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Cart
    {
        public Cart() { 
            products = new List<Product>();
            total = 0;
        }
        public List<Product> products { get; set; }
        public float total { get; set; }

        public void AddProduct(Product product)
        {
            this.products.Add(product);
            this.total += product.Price;
        }
    }
}
