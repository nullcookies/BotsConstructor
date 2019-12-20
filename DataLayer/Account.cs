using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    [Table("Accounts")]
    public class Account
    {
        //public Account()
        //{
        //	Bots = new HashSet<BotDB>();
        //}

        [Key]
        [Column("AccountId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Required]
        //public string Login { get; set; }
        [Required]
        public string Name { get; set; }
        
        public string Password { get; set; }

        public RoleType RoleType { get; set; }

        [Column("RoleTypeId")]
        [Required]
        public int RoleTypeId { get; set; }

        [DataType(DataType.EmailAddress)]
        //email не обязателен, тк возможен логин через телеграм
        public string Email { get;  set; }

        public int TelegramId { get; set; }

        public decimal Money { get; set; }

        public virtual ICollection<BotDB> Bots { get; set; }
    }
}