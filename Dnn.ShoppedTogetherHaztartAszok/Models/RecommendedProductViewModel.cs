namespace ShoppedTogetherHaztartasok.Dnn.Models
{
    public class RecommendedProductViewModel
    {
        public int Id { get; set; }
        public string Bvin { get; set; }
        public string Sku { get; set; }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; }
        public string ProductUrl { get; set; }

        public decimal? ListPrice { get; set; }
        public decimal? SitePrice { get; set; }

        public string UrlSlug { get; set; }

        public bool IsOnSale
        {
            get
            {
                return ListPrice.HasValue
                    && SitePrice.HasValue
                    && ListPrice.Value > SitePrice.Value;
            }
        }
    }
}