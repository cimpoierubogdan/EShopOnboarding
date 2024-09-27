using CartActorService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Common;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Common.Contracts;

namespace EShopAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        [HttpPost]
        public async Task<Order> PlaceOrder([FromQuery] int userId)
        {
            var cartActorId = new ActorId(userId);

            var cartProxy = ActorProxy.Create<ICartActorService>(
                cartActorId,
                new Uri("fabric:/EShopOnboarding/CartActorServiceActorService")
                );

            var cart = await cartProxy.GetCart(new CancellationToken());

            if (cart.products.Count() == 0)
            {
                throw new Exception("Cart is empty");
            }

            var order = new Order(0, userId, cart.products, cart.total);
            
            return await this.GetOrderProxy(order.id).SaveOrder(order);
        }

        [HttpGet]
        public async Task<Order> GetOrder([FromQuery] int orderId)
        {
            return await this.GetOrderProxy(orderId).GetOrder(orderId);
        }

        private IOrderServiceInterface GetOrderProxy(int orderId)
        {
            var partitionId = orderId % 3;

            return ServiceProxy.Create<IOrderServiceInterface>(
                new Uri("fabric:/EShopOnboarding/OrderService"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionId)
                );
        }
    }
}
