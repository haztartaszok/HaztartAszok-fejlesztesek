using System;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace ShoppedTogetherHaztartasok.Dnn.Models
{
    [TableName("hcc_Order")]
    [PrimaryKey(nameof(Id), AutoIncrement = true)]
    public class SourceOrder
    {
        public long Id { get; set; }
        public string bvin { get; set; }
        public bool IsPlaced { get; set; }
        public DateTime TimeOfOrder { get; set; }
        public DateTime LastUpdated { get; set; }
        public string OrderNumber { get; set; }
        public string UserId { get; set; }
        public string StatusCode { get; set; }
        public string StatusName { get; set; }
        public int StoreId { get; set; }
    }
}