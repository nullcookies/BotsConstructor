using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class BotLaunchRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BotId { get; set; }

        [Required]
        public BotStatus BotStatus { get; set; }

        [Required]
        public string BotStatusString {
            get
            {
                return BotStatus.ToString();
            }
            set{}
        }


        [Required]
        public DateTime Time { get; set; }
    }
    public enum BotStatus
    {
        STARTED,
        STOPPED
    }
}