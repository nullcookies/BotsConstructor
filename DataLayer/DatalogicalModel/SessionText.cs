using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    /// <summary>
    /// Тексты, которые были отправлены пользователем в специальный "инвентарь".
    /// </summary>
    [Table("SessionsTexts")]
    public class SessionText
    {
        [Key]
        [Required]
        [Column("SessionTextId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public int InventoryId { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public virtual Inventory Inventory { get; set; }
    }
}