using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class Order
    {
        public const int StatusPending = 1;
        public const int StatusPaid = 2;
        [DataMember]
        public int id {  get; set; }
        [DataMember]
        public int userId { get; set; }
        [DataMember]
        public List<Product> products { get; set; }
        [DataMember]
        public float total { get; set; }
        [DataMember]
        public int status { get; set; }

        public Order(int id, int userId, List<Product> products, float total)
        {
            this.id = id;
            this.userId = userId;
            this.products = products;
            this.total = total;
            this.status = StatusPending;
        }

    }
}
