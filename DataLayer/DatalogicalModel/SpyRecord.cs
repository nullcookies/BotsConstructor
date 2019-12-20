using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class SpyRecord
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime Time { get; set; }
        [Required]
        public string PathCurrent { get; set; }
        [Required]
        public string PathFrom { get; set; }
        [Required]
        public int AccountId { get; set; }
    }
}