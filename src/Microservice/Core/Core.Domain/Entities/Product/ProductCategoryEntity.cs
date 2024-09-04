using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Core.Domain
{
    [Table(name: "Product_ProductCategory")]
    public class ProductCategoryEntity : BaseEntity
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public CProductCategoryStatus Status { get; set; }

        #region view count
        public int DailyViewCount { get; set; } = 0;
        public int WeeklyViewCount { get; set; } = 0;
        public int MonthlyViewCount { get; set; } = 0;
        public int YearlyViewCount { get; set; } = 0;
        #endregion view count

        #region order count
        public int DailyOrderCount { get; set; } = 0;
        public int WeeklyOrderCount { get; set; } = 0;
        public int MonthlyOrderCount { get; set; } = 0;
        public int YearlyOrderCount { get; set; } = 0;

        #endregion order count

        #region InverseProperty 1 - n
        [InverseProperty(property: "ProductCategory")]
        public virtual ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();

        #endregion InverseProperty 1- n
    }
}