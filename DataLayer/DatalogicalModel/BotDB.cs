using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    [Table("Bots")]
    public class BotDB
    {
        //public BotDB()
        //{
        //	this.Orders = new HashSet<Order>();
        //}

        [Key]
        [Column("BotId")]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string BotName { get; set; }

        [Required]
        [ForeignKey("OwnerId")]
        public int OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public virtual Account Owner { get; set; }

        public string Markup { get; set; }

        public string BotType { get; set; }


        public virtual ICollection<Order> Orders { get; set; }
    }
}