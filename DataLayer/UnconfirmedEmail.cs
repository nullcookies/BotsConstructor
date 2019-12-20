using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    [Table("UnconfirmedEmails")]
    public class UnconfirmedEmail
    {
        public int Id { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public Guid GuidPasswordSentToEmail { get; set; }

    }
}