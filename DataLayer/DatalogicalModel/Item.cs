using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    /// <summary>
    /// Предметы (товары), заданные владельцем бота.
    /// </summary>
    [Table("Items")]
    public class Item
    {
        [Key]
        [Required]
        [Column("ItemId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Value { get; set; }

        [Required]
        [ForeignKey("BotId")]
        public int BotId { get; set; }

        [Required]
        [ForeignKey("BotId")]
        public virtual BotDB Bot { get; set; }
    }
}