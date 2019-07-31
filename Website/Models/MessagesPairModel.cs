using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class MessagesPairModel
    {
        public int NumberOfPair { get; set; }
        public string Textarea { get; set; }

        public string ChekedText { get; set; }
        public string ChekedVideo { get; set; }
        public string ChekedAudio { get; set; }
        public string ChekedLocation { get; set; }
    }
}
