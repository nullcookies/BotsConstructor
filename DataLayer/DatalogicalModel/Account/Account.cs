using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DataLayer
{
    [Table("Accounts")]
    public class Account
    {
        public Account()
        {
            Bots = new HashSet<BotDB>();
            OrderStatusGroups = new HashSet<OrderStatusGroup>();
        }

        [Key]
        [Column("AccountId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required] [StringLength(50, MinimumLength = 6)] public string Name { get; set; }
        public decimal Money { get; set; }
        [Required]
        public DateTime RegistrationDate { get; set; }
        
        public EmailLoginInfo EmailLoginInfo { get; set; }
        public TelegramLoginInfo TelegramLoginInfo { get; set; }

        public virtual ICollection<BotDB> Bots { get; set; }

        public virtual ICollection<OrderStatusGroup> OrderStatusGroups { get; set; }
    }
}