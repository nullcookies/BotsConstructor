using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    /// <summary>
    ///  Таблица хранит всю историю цен. 
    ///  Актуальное значение костант, конечно же хранится последней
    ///  записью.
    /// </summary>
    public class BotForSalesPrice
    {
        [Key]
        [Column("BotForSalesPriceId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public decimal MaxPrice { get; set; }
        [Required]
        public decimal MinPrice { get; set; }
        [Required]
        public decimal DailyPrice { get; set; }
        [Required]
        public decimal MagicParameter { get; set; }
        [Required]
        public DateTime DateTime { get; set; }

    }
}