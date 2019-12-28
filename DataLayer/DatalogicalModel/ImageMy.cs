using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
    [Table("Images")]
    public class ImageMy
    {
        public int Id { get; set; }
        public long ProductId { get; set; }
        public int BotId { get; set; }
        public string Name { get; set; }
        public byte[] Photo { get; set; }
    }
}