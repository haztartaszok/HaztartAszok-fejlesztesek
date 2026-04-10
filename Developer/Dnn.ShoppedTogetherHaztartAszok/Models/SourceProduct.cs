using DotNetNuke.ComponentModel.DataAnnotations;

namespace ShoppedTogetherHaztartasok.Dnn.Models
{
    [TableName("hcc_Product")]
    [PrimaryKey(nameof(Id), AutoIncrement = true)]
    public class SourceProduct
    {
        public int Id { get; set; }
        public string bvin { get; set; }
        public string SKU { get; set; }
        public string ProductTypeId { get; set; }
        public decimal? ListPrice { get; set; }
        public decimal? SitePrice { get; set; }
    }
}