using System.Collections.Generic;
using ShoppedTogetherHaztartasok.Dnn.Models;

namespace ShoppedTogetherHaztartasok.Dnn.Services
{
    public interface IShoppedTogetherService
    {
        ShoppedTogetherSyncRun StartSyncRun();
        void CompleteSyncRun(int syncRunId, int processedOrdersCount, int? lastProcessedOrderId);
        void FailSyncRun(int syncRunId, string errorMessage);

        ShoppedTogetherSyncRun GetLastSuccessfulSyncRun();

        IEnumerable<ShoppedTogetherProductPair> GetPairsForProduct(int productId);

        void UpsertPairCount(int productAId, int productBId, int incrementBy);

        void InitializeAllPairs();

        void PopulatePairCountsFromOrders();

        void RunIncrementalSync();

    }
}