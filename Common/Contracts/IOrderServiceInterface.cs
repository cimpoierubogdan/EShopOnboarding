using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Contracts
{
    public interface IOrderServiceInterface: IService
    {
        public Task<Order> SaveOrder(Order order);
        public Task<Order> GetOrder(int id);
    }
}
