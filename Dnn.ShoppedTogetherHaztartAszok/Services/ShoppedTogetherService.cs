using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using DotNetNuke.Data;
using ShoppedTogetherHaztartasok.Dnn.Models;
using Hotcakes.Commerce;
using DotNetNuke.Entities.Portals;
using Hotcakes.Commerce.Urls;

namespace ShoppedTogetherHaztartasok.Dnn.Services
{
    public class ShoppedTogetherService : IShoppedTogetherService
    {
        private static readonly CultureInfo HungarianCulture = CultureInfo.GetCultureInfo("hu-HU");

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

        public void RunIncrementalSync()
        {
            var lastRun = GetLastSuccessfulSyncRun();

            if (lastRun == null)
            {
                InitializeAllPairs();
            }

            PopulatePairCountsFromOrders();
        }

        public IEnumerable<int> GetTopRecommendedProductIds(IEnumerable<int> cartProductIds, int topN)
        {
            var cartIds = cartProductIds
                .Distinct()
                .ToList();

            var scores = new Dictionary<int, int>();

            using (var ctx = DataContext.Instance())
            {
                var pairRepo = ctx.GetRepository<ShoppedTogetherProductPair>();

                foreach (var cartProductId in cartIds)
                {
                    var pairs = pairRepo.Find(
                        "WHERE ProductAId = @0 OR ProductBId = @0",
                        cartProductId).ToList();

                    foreach (var pair in pairs)
                    {
                        int recommendedProductId =
                            pair.ProductAId == cartProductId
                                ? pair.ProductBId
                                : pair.ProductAId;

                        if (cartIds.Contains(recommendedProductId))
                        {
                            continue;
                        }

                        if (!scores.ContainsKey(recommendedProductId))
                        {
                            scores[recommendedProductId] = 0;
                        }

                        scores[recommendedProductId] += pair.TogetherCount;
                    }
                }
            }

            return scores
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key)
                .Take(topN)
                .Select(x => x.Key)
                .ToList();
        }

        public IEnumerable<int> GetTopSellingProductIds(int topN)
        {
            using (var ctx = DataContext.Instance())
            {
                var orderRepo = ctx.GetRepository<SourceOrder>();
                var lineItemRepo = ctx.GetRepository<SourceLineItem>();
                var productRepo = ctx.GetRepository<SourceProduct>();

                var allProducts = productRepo.Get().ToList();

                var productBvinToId = allProducts
                    .Where(p => !string.IsNullOrEmpty(p.bvin))
                    .GroupBy(p => p.bvin)
                    .ToDictionary(g => g.Key, g => g.First().Id);

                var placedOrderBvins = orderRepo.Find("WHERE IsPlaced = 1")
                    .Select(o => o.bvin)
                    .ToList();

                var scores = new Dictionary<int, int>();

                foreach (var orderBvin in placedOrderBvins)
                {
                    var items = lineItemRepo.Find("WHERE OrderBvin = @0", orderBvin).ToList();

                    foreach (var item in items)
                    {
                        if (string.IsNullOrEmpty(item.ProductId))
                        {
                            continue;
                        }

                        if (!productBvinToId.ContainsKey(item.ProductId))
                        {
                            continue;
                        }

                        var productId = productBvinToId[item.ProductId];

                        if (!scores.ContainsKey(productId))
                        {
                            scores[productId] = 0;
                        }

                        scores[productId] += item.Quantity;
                    }
                }

                return scores
                    .OrderByDescending(x => x.Value)
                    .ThenBy(x => x.Key)
                    .Take(topN)
                    .Select(x => x.Key)
                    .ToList();
            }
        }

        public IEnumerable<RecommendedProductViewModel> GetRecommendedProductsByIds(IEnumerable<int> productIds)
        {
            var ids = productIds
                .Distinct()
                .ToList();

            var result = new List<RecommendedProductViewModel>();

            using (var ctx = DataContext.Instance())
            {
                var productRepo = ctx.GetRepository<SourceProduct>();

                var sourceProducts = productRepo.Get()
                    .Where(p => ids.Contains(p.Id))
                    .ToList();

                var sourceProductsById = sourceProducts.ToDictionary(p => p.Id, p => p);

                var app = HotcakesApplication.Current;

                foreach (var id in ids)
                {
                    if (!sourceProductsById.ContainsKey(id))
                    {
                        continue;
                    }

                    var sourceProduct = sourceProductsById[id];

                    if (string.IsNullOrWhiteSpace(sourceProduct.bvin))
                    {
                        continue;
                    }

                    var hcProduct = app.CatalogServices.Products.Find(sourceProduct.bvin);

                    if (hcProduct == null)
                    {
                        continue;
                    }


                    var imageUrl = string.Empty;
                    var portalId = PortalSettings.Current != null ? PortalSettings.Current.PortalId : 0;

                    if (!string.IsNullOrWhiteSpace(hcProduct.ImageFileMedium) && !string.IsNullOrWhiteSpace(hcProduct.Bvin))
                    {
                        imageUrl = string.Format(
                            "/Portals/{0}/Hotcakes/Data/products/{1}/medium/{2}",
                            portalId,
                            hcProduct.Bvin,
                            hcProduct.ImageFileMedium);
                    }
                    else if (!string.IsNullOrWhiteSpace(hcProduct.ImageFileSmall) && !string.IsNullOrWhiteSpace(hcProduct.Bvin))
                    {
                        imageUrl = string.Format(
                            "/Portals/{0}/Hotcakes/Data/products/{1}/small/{2}",
                            portalId,
                            hcProduct.Bvin,
                            hcProduct.ImageFileSmall);
                    }

                    var productUrl = string.Empty;

                    if (!string.IsNullOrWhiteSpace(hcProduct.UrlSlug))
                    {
                        productUrl = "/hotcakesstore/product-viewer/" + hcProduct.UrlSlug;
                    }

                    var addToCartUrl = string.Empty;
                    var cartUrl = HccUrlBuilder.RouteHccUrl(HccRoute.Cart);
                    if (!string.IsNullOrWhiteSpace(cartUrl) && !string.IsNullOrWhiteSpace(sourceProduct.SKU))
                    {
                        addToCartUrl = cartUrl
                            + (cartUrl.Contains("?") ? "&" : "?")
                            + "AddSku=" + HttpUtility.UrlEncode(sourceProduct.SKU)
                            + "&AddSkuQty=1";
                    }

                    result.Add(new RecommendedProductViewModel
                    {
                        Id = sourceProduct.Id,
                        Bvin = sourceProduct.bvin,
                        Sku = sourceProduct.SKU,
                        Name = hcProduct.ProductName,
                        ShortDescription = hcProduct.ShortDescription,
                        ImageUrl = imageUrl,
                        ProductUrl = productUrl,
                        AddToCartUrl = addToCartUrl,
                        UrlSlug = hcProduct.UrlSlug,
                        ListPrice = sourceProduct.ListPrice,
                        SitePrice = sourceProduct.SitePrice,
                        ListPriceText = sourceProduct.ListPrice.HasValue
                            ? sourceProduct.ListPrice.Value.ToString("N0", HungarianCulture) + " Ft"
                            : string.Empty,
                        SitePriceText = sourceProduct.SitePrice.HasValue
                            ? sourceProduct.SitePrice.Value.ToString("N0", HungarianCulture) + " Ft"
                            : string.Empty,
                        CustomPriceText = hcProduct.SitePriceOverrideText,
                        HasOptions = hcProduct.HasOptions(),
                        IsGiftCard = hcProduct.IsGiftCard,
                        IsBundle = hcProduct.IsBundle,
                        IsUserSuppliedPrice = hcProduct.IsUserSuppliedPrice,
                        AllowUpcharge = hcProduct.AllowUpcharge
                    });
                }
            }

            return result;
        }

    }
}
