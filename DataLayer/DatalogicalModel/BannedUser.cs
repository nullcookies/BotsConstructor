using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class BannedUser
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string BotUsername { get; set; }
        [Required]
        public int UserTelegramId { get; set; }
    }
}