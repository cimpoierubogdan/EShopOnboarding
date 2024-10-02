using CartActorService.Interfaces;
using Common;
using EShopAPI.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using ProductActorService.Interfaces;

namespace EShopAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        [HttpGet]
        public async Task<Cart> GetCartByUserId([FromQuery] int userId)
        {
            var actorId = new ActorId(userId);

            var proxy = ActorProxy.Create<ICartActorService>(
                actorId,
                new Uri("fabric:/EShopOnboarding/CartActorServiceActorService")
                );

            var cart = await proxy.GetCart(new CancellationToken());

            return cart;
        }

        [HttpPost]
        public async Task AddProduct([FromBody] AddToCartRequest addToCartRequest)
        {
            var productActorId = new ActorId(addToCartRequest.productId);
            var userActorId = new ActorId(addToCartRequest.userId);

            var productProxy = ActorProxy.Create<IProductActorService>(
                productActorId,
                new Uri("fabric:/EShopOnboarding/ProductActorServiceActorService")
                );

            var product = await productProxy.GetProduct(new CancellationToken());

            if (product.Stock < 1)
            {
                throw new Exception("Product out of stock");
            }

            var cartProxy = ActorProxy.Create<ICartActorService>(
                userActorId,
                new Uri("fabric:/EShopOnboarding/CartActorServiceActorService")
                );

            var cart = await cartProxy.GetCart(new CancellationToken());
            cart.AddProduct( product );
            await cartProxy.SetCart(cart, new CancellationToken());
            product.Stock--;
            await productProxy.AddProduct(product, new CancellationToken());
        }
    }
}
