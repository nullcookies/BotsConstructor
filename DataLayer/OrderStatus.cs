using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    /// <summary>
    /// Статус заказа.
    /// </summary>
    [Table("OrderStatuses")]
    public class OrderStatus
    {
        [Key]
        [Required]
        [Column("OrderStatusId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("OrderStatusGroupId")]
        public int GroupId { get; set; }

        [Required]
        [ForeignKey("GroupId")]
        public virtual OrderStatusGroup Group { get; set; }

        [Required]
        public string Name { get; set; }

        public string Message { get; set; }

        [Required]
        public bool IsOld { get; set; }
    }
}