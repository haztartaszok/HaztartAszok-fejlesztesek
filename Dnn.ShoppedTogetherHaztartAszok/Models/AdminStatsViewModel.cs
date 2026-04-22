using System.Collections.Generic;

namespace ShoppedTogetherHaztartAszok.Dnn.Dnn.ShoppedTogetherHaztartAszok.Models
{
    public class AdminStatsViewModel
    {
        public int? SelectedProductId { get; set; }
        public List<ProductItem> Products { get; set; }

        public List<RelatedProductStat> RelatedProducts { get; set; }

        public string SelectedProductName { get; set; }
    }
}