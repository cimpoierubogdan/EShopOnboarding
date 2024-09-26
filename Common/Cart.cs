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
        }
        public List<Product> products { get; set; }
        public float total { get; set; }
    }
}
