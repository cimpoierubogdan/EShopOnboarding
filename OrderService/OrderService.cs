using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common.Contracts;
using Common;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using System.Fabric.Description;
using System.Collections.ObjectModel;
using Common.Util;

namespace OrderService
{
    internal sealed class OrderService : StatefulService, IOrderServiceInterface
    {
        public OrderService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<Order> SaveOrder(Order order)
        {
            var orderDict = await this.GetOrderDictionary();

            using (var tx = this.StateManager.CreateTransaction())
            {
                if (0 == order.id)
                {
                    order.id = (int)await orderDict.GetCountAsync(tx);
                    order.id++;
                    await orderDict.AddAsync(tx, order.id, order);
                    await tx.CommitAsync();

                    return order;
                }
                var oldOrder = await orderDict.TryGetValueAsync(tx, order.id);
                await orderDict.TryUpdateAsync(tx, order.id, order, oldOrder.Value);
                
                await tx.CommitAsync();
            }

            return order;
        }

        public async Task<Order> GetOrder(int id)
        {
            var orderDict = await this.GetOrderDictionary();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var order = await orderDict.TryGetValueAsync(tx, id);

                return order.Value;
            }

            throw new Exception("could not fetch order");
        }

        public async Task AddIPN(int orderId)
        {
            var queue = await this.GetIPNQueue();

            using (var tx = this.StateManager.CreateTransaction())
            {
                await queue.EnqueueAsync(tx, orderId);
                await tx.CommitAsync();
            }
        }

        public async Task<int> GetNextIPN()
        {
            var queue = await this.GetIPNQueue();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var ipn = await queue.TryDequeueAsync(tx);
                await tx.CommitAsync();

                return ipn.Value;
            }

            throw new Exception("no ipn available");
        }

        private async Task<IReliableDictionary<int, Order>> GetOrderDictionary()
        {
            return await this.StateManager.GetOrAddAsync<IReliableDictionary<int, Order>>("orders");
        }

        private async Task<IReliableQueue<int>> GetIPNQueue()
        {
            return await this.StateManager.GetOrAddAsync<IReliableQueue<int>>("ipns");
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            //var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            FabricClient fabricClient = new FabricClient();
            StatefulServiceLoadMetricDescription ordersCount = new StatefulServiceLoadMetricDescription();
            ordersCount.Name = "ordersCount";
            ordersCount.Weight = ServiceLoadMetricWeight.High;

            StatefulServiceUpdateDescription description = new StatefulServiceUpdateDescription();
            description.Metrics = new MetricsCollection();
            description.Metrics.Add(ordersCount);

            await fabricClient.ServiceManager.UpdateServiceAsync(new Uri(this.Context.ServiceName.AbsoluteUri), description);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var orderDict = await this.GetOrderDictionary();
                    int orderCount = (int)(await orderDict.GetCountAsync(tx));
                    ServiceEventSource.Current.ServiceMessage(
                        this.Context,
                        "order count {0} on partition {1}",
                        orderCount,
                        this.Partition.PartitionInfo.Id
                        );
                    this.Partition.ReportLoad(new List<LoadMetric> { new LoadMetric("ordersCount", orderCount) });

                    //var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    //ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                    //    result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    //await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
