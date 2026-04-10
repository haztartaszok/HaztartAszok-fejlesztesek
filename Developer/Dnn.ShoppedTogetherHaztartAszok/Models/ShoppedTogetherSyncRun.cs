using System;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace ShoppedTogetherHaztartasok.Dnn.Models
{
    [TableName("ShoppedTogetherSyncRuns")]
    [PrimaryKey(nameof(SyncRunId), AutoIncrement = true)]
    public class ShoppedTogetherSyncRun
    {
        public int SyncRunId { get; set; }
        public DateTime StartedOnUtc { get; set; }
        public DateTime? FinishedOnUtc { get; set; }
        public bool WasSuccessful { get; set; }
        public int? LastProcessedOrderId { get; set; }
        public int ProcessedOrdersCount { get; set; }
        public string ErrorMessage { get; set; }
    }
}