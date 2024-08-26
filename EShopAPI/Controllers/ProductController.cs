using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using ProductActorService.Interfaces;

namespace EShopAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        public async Task<Product> GetProductById([FromQuery] int id)
        {
            var actorId = new ActorId(id);

            var proxy = ActorProxy.Create<IProductActorService>(
                actorId,
                new Uri("fabric:/EShopOnboarding/ProductActorServiceActorService")
                );

            var product = await proxy.GetProduct(new CancellationToken());

            return product;
        }

        [HttpPost]
        public async Task AddProduct([FromBody] Product product)
        {
            var actorId = new ActorId(product.Id);

            var proxy = ActorProxy.Create<IProductActorService>(
                actorId,
                new Uri("fabric:/EShopOnboarding/ProductActorServiceActorService")
                );

            await proxy.AddProduct(product, new CancellationToken());
        }
    }
}
