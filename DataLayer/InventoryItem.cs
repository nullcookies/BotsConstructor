using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    /// <summary>
    /// Предметы, которые привязаны к пользователю (товары в корзине).
    /// </summary>
    [Table("InventoryItems")]
    public class InventoryItem
    {
        [Key]
        [Required]
        [Column("InventoryItemId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int Count { get; set; }

        [Required]
        [ForeignKey("ItemId")]
        public int ItemId { get; set; }

        [Required]
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public int InventoryId { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public virtual Inventory Inventory { get; set; }
    }
}