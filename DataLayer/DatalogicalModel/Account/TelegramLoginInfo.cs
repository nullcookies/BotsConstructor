using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    [Table("TelegramLoginInfo")]
    public class TelegramLoginInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int TelegramId { get; set; }
        [Required]
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public Account Account { get; set; }
    }
}