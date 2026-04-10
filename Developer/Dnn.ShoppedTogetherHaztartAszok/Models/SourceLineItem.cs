using System;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace ShoppedTogetherHaztartasok.Dnn.Models
{
    [TableName("hcc_LineItem")]
    [PrimaryKey(nameof(Id), AutoIncrement = true)]
    public class SourceLineItem
    {
        public long Id { get; set; }
        public DateTime LastUpdated { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string OrderBvin { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? AdjustedPrice { get; set; }
        public decimal? LineTotal { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public string StatusCode { get; set; }
        public string StatusName { get; set; }
        public int StoreId { get; set; }
    }
}