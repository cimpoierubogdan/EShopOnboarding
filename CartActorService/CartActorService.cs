using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using CartActorService.Interfaces;
using Common;

namespace CartActorService
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class CartActorService : Actor, ICartActorService
    {
        private string CartStateName = "CartState";
        private IActorTimer _emptyCartTimer;

        public CartActorService(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        public async Task<Cart> GetCart(CancellationToken cancellationToken)
        {
            var cart = await this.StateManager.GetOrAddStateAsync<Cart>(CartStateName, new Cart(), cancellationToken);

            return cart;
        }

        public async Task SetCart(Cart cart, CancellationToken cancellationToken)
        {
            await this.StateManager.AddOrUpdateStateAsync(CartStateName, cart, (k, v) => cart, cancellationToken);

            await this.StateManager.SaveStateAsync(cancellationToken);

            this.ResetTimer();
        }

        protected override Task OnActivateAsync()
        {
            this.RegisterTimer();

            return base.OnActivateAsync();
        }

        protected override Task OnDeactivateAsync()
        {
            if (_emptyCartTimer != null)
            {
                this.UnregisterTimer();
            }

            return base.OnDeactivateAsync();
        }

        private async Task EmptyCart(object state)
        {
            var cancellationToken = new CancellationToken();
            var cart = await this.StateManager.GetStateAsync<Cart>(CartStateName, cancellationToken);

            cart.products.Clear();
            cart.total = 0;

            await this.StateManager.AddOrUpdateStateAsync(CartStateName, cart, (k, v) => cart, cancellationToken);
        }

        private void RegisterTimer()
        {
            _emptyCartTimer = RegisterTimer(
                    EmptyCart,
                    null,
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromMinutes(5)
                );
        }

        private void UnregisterTimer()
        {
            UnregisterTimer(_emptyCartTimer);
        }

        private void ResetTimer()
        {
            this.UnregisterTimer();
            this.RegisterTimer();
        }
    }
}
