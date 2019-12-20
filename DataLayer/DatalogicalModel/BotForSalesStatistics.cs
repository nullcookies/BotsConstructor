using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    public class BotForSalesStatistics
    {
        [Key]
        public int BotId { get; set; }

        [ForeignKey("BotId")]
        public BotDB Bot { get; set; } 

        [Required]
        public long NumberOfOrders          { get; set; }
        [Required]
        public int  NumberOfUniqueUsers     { get; set; }
        [Required]
        public long NumberOfUniqueMessages  { get; set; }
    }
}