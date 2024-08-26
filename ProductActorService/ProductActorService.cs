using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using ProductActorService.Interfaces;
using Common;

namespace ProductActorService
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class ProductActorService : Actor, IProductActorService
    {
        private string ProductStateName = "ProductState";

        /// <summary>
        /// Initializes a new instance of ProductActorService
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public ProductActorService(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        public async Task AddProduct(Product product, CancellationToken cancellationToken)
        {
            await this.StateManager.AddOrUpdateStateAsync(ProductStateName, product, (k, v) => product, cancellationToken);

            await this.StateManager.SaveStateAsync(cancellationToken);
        }

        public async Task<Product> GetProduct(CancellationToken cancellationToken)
        {
            var product = await this.StateManager.GetStateAsync<Product>(ProductStateName, cancellationToken);

            return product;
        }
    }
}
