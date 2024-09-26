using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace CartActorService.Interfaces
{
    public interface ICartActorService : IActor
    {
        Task SetCart(Cart cart, CancellationToken cancellationToken);

        Task<Cart> GetCart(CancellationToken cancellationToken);
    }
}
