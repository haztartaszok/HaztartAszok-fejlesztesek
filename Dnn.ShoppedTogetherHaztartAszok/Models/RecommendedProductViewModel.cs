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
        public string AddToCartUrl { get; set; }
        public string ListPriceText { get; set; }
        public string SitePriceText { get; set; }

        public decimal? ListPrice { get; set; }
        public decimal? SitePrice { get; set; }

        public string UrlSlug { get; set; }
        public string CustomPriceText { get; set; }
        public bool HasOptions { get; set; }
        public bool IsGiftCard { get; set; }
        public bool IsBundle { get; set; }
        public bool IsUserSuppliedPrice { get; set; }
        public bool AllowUpcharge { get; set; }

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
