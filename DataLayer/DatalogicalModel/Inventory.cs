using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    /// <summary>
    /// Множества элементов, принадлежащих одной сессии (например, заказы или содержимое сундуков в RPG).
    /// </summary>
    [Table("Inventories")]
    public class Inventory
    {
        [Key]
        [Required]
        [Column("InventoryId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SessionId { get; set; }

        public virtual ICollection<InventoryItem> Items { get; set; }

        public virtual ICollection<SessionText> Texts { get; set; }

        public virtual ICollection<SessionFile> Files { get; set; }

        [ForeignKey("InventoryId")]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Inventory Parent { get; set; }

        public virtual ICollection<Inventory> Children { get; set; }
    }
}