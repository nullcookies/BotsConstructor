using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    /// <summary>
    /// Файлы, которые были отправлены пользователем в специальный "инвентарь".
    /// </summary>
    [Table("SessionsFiles")]
    public class SessionFile
    {
        [Key]
        [Required]
        [Column("SessionFileId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string FileId { get; set; }

        // Некоторые файлы не имеют превью
        public string PreviewId { get; set; }

        // Описание пользователь может и не добавить
        public string Description { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public int InventoryId { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public virtual Inventory Inventory { get; set; }
    }
}