using DotNetNuke.ComponentModel.DataAnnotations;

namespace ShoppedTogetherHaztartasok.Dnn.Models
{
    [TableName("ShoppedTogetherProductPairs")]
    [PrimaryKey(nameof(PairId), AutoIncrement = true)]
    public class ShoppedTogetherProductPair
    {
        public int PairId { get; set; }
        public int ProductAId { get; set; }
        public int ProductBId { get; set; }
        public int TogetherCount { get; set; }
    }
}