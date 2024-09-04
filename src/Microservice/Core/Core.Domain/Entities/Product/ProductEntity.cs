using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain
{
    /*
        About SEO is : Search Engine Optimizations.
    */
    public class ProductEntity : BaseEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string SeoAlias { get; set; } = string.Empty;
        public string SeoTitle { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        public int Stock { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;
        public CProductStatus Status { get; set; }

        #region support Statistical
        public int DailyViewCount { get; set; } = 0;
        public int WeeklyViewCount { get; set; } = 0;
        public int MonthlyViewCount { get; set; } = 0;
        public int YearlyViewCount { get; set; } = 0;

        public int DailyOrderCount { get; set; } = 0;
        public int WeeklyOrderCount { get; set; } = 0;
        public int MonthlyOrderCount { get; set; } = 0;
        public int YearlyOrderCount { get; set; } = 0;
        #endregion support Statistical

        #region Json property
        public string DiscountPropertyJson { get; set; } = string.Empty;
        #endregion Json property

        #region a
        public string ProductCategoryId { get; set; } = string.Empty;
        [ForeignKey(name: "ProductCategoryId")]
        [InverseProperty(property: "Products")]
        public virtual ProductCategoryEntity ProductCategory { get; set; } = null!;
        #endregion a
    }
}