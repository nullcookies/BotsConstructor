using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    [Table("EmailLoginInfo")]
    public class EmailLoginInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get;  set; }
        [Required]
        public string Password { get; set; }
        [ForeignKey("AccountId")]
        public Account Account { get; set; }
    }
}