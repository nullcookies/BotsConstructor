using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    [Table("RoleTypesMy")]
    public class RoleType
    {
        //public RoleType()
        //{
        //	this.Accounts = new HashSet<Account>();
        //}

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}