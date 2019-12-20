using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    [Table("AccountsToResetPassword")]
    public class AccountToResetPassword
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int AccountId { get; set; }

        [ForeignKey("AccountId")]
        public Account Account{get;set;}
        
        [Required]
        public Guid GuidPasswordSentToEmail { get; set; }

    }
}