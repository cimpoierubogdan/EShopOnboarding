using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class Product
    {
        [DataMember]
        public int Id {  get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Stock { get; set; }
        [DataMember]
        public float Price { get; set; }
    }
}
