using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    [Table("Moderators")]
    public class Moderator
    {
        [Key]
        [Column("ModeratorId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int BotId { get; set; }
        [ForeignKey("BotId")]
        public BotDB BotDB { get; set; }

        [Required]
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public Account Account { get; set; }
    }
}