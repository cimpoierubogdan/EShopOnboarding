using Common;
using Common.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace EShopAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IPNController : ControllerBase
    {
        [HttpPost]
        public async Task UpdateOrder([FromQuery] int orderId)
        {
            var partitionId = orderId % 3;

            var orderProxy = ServiceProxy.Create<IOrderServiceInterface>(
                new Uri("fabric:/EShopOnboarding/OrderService"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId)
                );

            await orderProxy.AddIPN(orderId);
        }
    }
}
