using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    [Table("RouteRecords")]
    public class RouteRecord
    {
        [Key]
        public int BotId { get; set; }

        [ForeignKey("BotId")]
        public BotDB Bot { get; set; }

        /// <summary>
        /// Хранит ссылку на сервер леса.
        /// Например http://localhost:8080/Home/ http://15.41.87.12/Home/
        /// </summary>
        [Required]
        public string ForestLink { get; set; }
    }
}