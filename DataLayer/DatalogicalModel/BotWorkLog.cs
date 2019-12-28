using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    /// <summary>
    /// В этой таблице хранятся записи о работе ботов
    /// На её основе определяется работал бот в этот день или нет
    /// </summary>
    public class BotWorkLog
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int BotId { get; set; }
        [Required]
        public DateTime InspectionTime { get; set; }
    }
}