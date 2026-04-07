using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Data;
using ShoppedTogetherHaztartasok.Dnn.Models;

namespace ShoppedTogetherHaztartasok.Dnn.Services
{
    public class ShoppedTogetherService : IShoppedTogetherService
    {
        public ShoppedTogetherSyncRun StartSyncRun()
        {
            var run = new ShoppedTogetherSyncRun
            {
                StartedOnUtc = DateTime.UtcNow,
                WasSuccessful = false,
                ProcessedOrdersCount = 0,
                ErrorMessage = null,
                LastProcessedOrderId = null,
                FinishedOnUtc = null
            };

            using (var ctx = DataContext.Instance())
            {
                var repo = ctx.GetRepository<ShoppedTogetherSyncRun>();
                repo.Insert(run);
            }

            return run;
        }

        public void CompleteSyncRun(int syncRunId, int processedOrdersCount, int? lastProcessedOrderId)
        {
            using (var ctx = DataContext.Instance())
            {
                var repo = ctx.GetRepository<ShoppedTogetherSyncRun>();
                var run = repo.GetById(syncRunId);

                if (run == null)
                {
                    throw new InvalidOperationException($"Sync run not found: {syncRunId}");
                }

                run.FinishedOnUtc = DateTime.UtcNow;
                run.WasSuccessful = true;
                run.ProcessedOrdersCount = processedOrdersCount;
                run.LastProcessedOrderId = lastProcessedOrderId;
                run.ErrorMessage = null;

                repo.Update(run);
            }
        }

        public void FailSyncRun(int syncRunId, string errorMessage)
        {
            using (var ctx = DataContext.Instance())
            {
                var repo = ctx.GetRepository<ShoppedTogetherSyncRun>();
                var run = repo.GetById(syncRunId);

                if (run == null)
                {
                    throw new InvalidOperationException($"Sync run not found: {syncRunId}");
                }

                run.FinishedOnUtc = DateTime.UtcNow;
                run.WasSuccessful = false;
                run.ErrorMessage = errorMessage;

                repo.Update(run);
            }
        }

        public ShoppedTogetherSyncRun GetLastSuccessfulSyncRun()
        {
            using (var ctx = DataContext.Instance())
            {
                return ctx.GetRepository<ShoppedTogetherSyncRun>()
                    .Find("WHERE WasSuccessful = @0 ORDER BY FinishedOnUtc DESC", true)
                    .FirstOrDefault();
            }
        }

        public IEnumerable<ShoppedTogetherProductPair> GetPairsForProduct(int productId)
        {
            using (var ctx = DataContext.Instance())
            {
                return ctx.GetRepository<ShoppedTogetherProductPair>()
                    .Find("WHERE ProductAId = @0 OR ProductBId = @0", productId)
                    .ToList();
            }
        }

        public void UpsertPairCount(int productAId, int productBId, int incrementBy)
        {
            // mindig rendezett sorrend (A < B)
            if (productAId > productBId)
            {
                var temp = productAId;
                productAId = productBId;
                productBId = temp;
            }

            using (var ctx = DataContext.Instance())
            {
                var repo = ctx.GetRepository<ShoppedTogetherProductPair>();

                var existing = repo.Find("WHERE ProductAId = @0 AND ProductBId = @1", productAId, productBId)
                                   .FirstOrDefault();

                if (existing != null)
                {
                    existing.TogetherCount += incrementBy;
                    repo.Update(existing);
                }
                else
                {
                    repo.Insert(new ShoppedTogetherProductPair
                    {
                        ProductAId = productAId,
                        ProductBId = productBId,
                        TogetherCount = incrementBy
                    });
                }
            }
        }

        private static void NormalizePair(ref int productAId, ref int productBId)
        {
            if (productAId > productBId)
            {
                var temp = productAId;
                productAId = productBId;
                productBId = temp;
            }
        }

        public void InitializeAllPairs()
        {
            using (var ctx = DotNetNuke.Data.DataContext.Instance())
            {
                var sourceProductRepo = ctx.GetRepository<SourceProduct>();
                var pairRepo = ctx.GetRepository<ShoppedTogetherProductPair>();

                var allProductIds = sourceProductRepo.Get()
                    .Select(p => p.Id)
                    .Distinct()
                    .OrderBy(id => id)
                    .ToList();

                for (int i = 0; i < allProductIds.Count; i++)
                {
                    for (int j = i + 1; j < allProductIds.Count; j++)
                    {
                        var productAId = allProductIds[i];
                        var productBId = allProductIds[j];

                        var existingPair = pairRepo.Find(
                                "WHERE ProductAId = @0 AND ProductBId = @1",
                                productAId,
                                productBId)
                            .FirstOrDefault();

                        if (existingPair == null)
                        {
                            pairRepo.Insert(new ShoppedTogetherProductPair
                            {
                                ProductAId = productAId,
                                ProductBId = productBId,
                                TogetherCount = 0
                            });
                        }
                    }
                }
            }
        }

        public void PopulatePairCountsFromOrders()
        {
            using (var ctx = DataContext.Instance())
            {
                var orderRepo = ctx.GetRepository<SourceOrder>();
                var lineItemRepo = ctx.GetRepository<SourceLineItem>();
                var productRepo = ctx.GetRepository<SourceProduct>();
                var pairRepo = ctx.GetRepository<ShoppedTogetherProductPair>();

                var allProducts = productRepo.Get().ToList();

                var productBvinToId = allProducts
                    .Where(p => !string.IsNullOrEmpty(p.bvin))
                    .GroupBy(p => p.bvin)
                    .ToDictionary(g => g.Key, g => g.First().Id);

                var lastRun = ctx.GetRepository<ShoppedTogetherSyncRun>()
                    .Find("WHERE WasSuccessful = 1 ORDER BY SyncRunId DESC")
                    .FirstOrDefault();

                List<SourceOrder> placedOrders;

                if (lastRun != null && lastRun.LastProcessedOrderId.HasValue)
                {
                    placedOrders = orderRepo.Find(
                        "WHERE IsPlaced = 1 AND Id > @0",
                        lastRun.LastProcessedOrderId.Value).ToList();
                }
                else
                {
                    placedOrders = orderRepo.Find("WHERE IsPlaced = 1").ToList();
                }

                foreach (var order in placedOrders)
                {
                    var productIdsInOrder = lineItemRepo.Find("WHERE OrderBvin = @0", order.bvin)
                        .Select(li => li.ProductId)
                        .Where(productBvin => !string.IsNullOrEmpty(productBvin) && productBvinToId.ContainsKey(productBvin))
                        .Select(productBvin => productBvinToId[productBvin])
                        .Distinct()
                        .OrderBy(id => id)
                        .ToList();

                    for (int i = 0; i < productIdsInOrder.Count; i++)
                    {
                        for (int j = i + 1; j < productIdsInOrder.Count; j++)
                        {
                            int productAId = productIdsInOrder[i];
                            int productBId = productIdsInOrder[j];

                            var pair = pairRepo.Find(
                                    "WHERE ProductAId = @0 AND ProductBId = @1",
                                    productAId,
                                    productBId)
                                .FirstOrDefault();

                            if (pair != null)
                            {
                                pair.TogetherCount += 1;
                                pairRepo.Update(pair);
                            }
                        }
                    }
                }

                if (placedOrders.Any())
                {
                    var lastOrderId = placedOrders.Max(o => o.Id);

                    ctx.GetRepository<ShoppedTogetherSyncRun>().Insert(
                        new ShoppedTogetherSyncRun
                        {
                            StartedOnUtc = DateTime.UtcNow,
                            FinishedOnUtc = DateTime.UtcNow,
                            WasSuccessful = true,
                            LastProcessedOrderId = (int)lastOrderId,
                            ProcessedOrdersCount = placedOrders.Count
                        });
                }
            }
        }

    }
}