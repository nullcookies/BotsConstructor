using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class PingRecord
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Url { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        [Required]
        public bool IsOk { get; set; }
        public string Description { get; set; }
    }
}