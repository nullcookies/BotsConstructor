using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    /// <summary>
    /// Заказ.
    /// </summary>
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("OrderId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        public string SenderNickname { get; set; }

        [Required]
        [ForeignKey("BotId")]
        public int BotId { get; set; }

        [Required]
        [ForeignKey("BotId")]
        public virtual BotDB Bot { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        [ForeignKey("OrderStatusId")]
        public int? OrderStatusId { get; set; }

        [ForeignKey("OrderStatusId")]
        public virtual OrderStatus OrderStatus { get; set; }

        [Required]
        [ForeignKey("OrderStatusGroupId")]
        public int OrderStatusGroupId { get; set; }

        [Required]
        [ForeignKey("OrderStatusGroupId")]
        public virtual OrderStatusGroup OrderStatusGroup { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public int ContainerId { get; set; }

        [Required]
        [ForeignKey("ContainerId")]
        public virtual Inventory Container { get; set; }
    }
}