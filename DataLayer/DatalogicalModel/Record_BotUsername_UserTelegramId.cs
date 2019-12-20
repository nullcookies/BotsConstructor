using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class Record_BotUsername_UserTelegramId
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string BotUsername { get; set; }
        [Required]
        public int BotUserTelegramId { get; set; }
    }
}