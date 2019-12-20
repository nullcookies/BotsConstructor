using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    /// <summary>
    /// Группа из статусов заказа определённого владельца, из которой можно выбирать статусы.
    /// </summary>
    [Table("OrderStatusesGroups")]
    public class OrderStatusGroup
    {
        [Key]
        [Required]
        [Column("OrderStatusGroupId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [ForeignKey("AccountId")]
        public int OwnerId { get; set; }

        [Required]
        [ForeignKey("OwnerId")]
        public virtual Account Owner { get; set; }

        [Required]
        public bool IsOld { get; set; }

        public virtual ICollection<OrderStatus> OrderStatuses { get; set; }
    }
}