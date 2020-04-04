using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.Models
{
    public class BusTemp
    {
        public int Id { get; set; }
        public string from_ { get; set; }
        public string to_ { get; set; }
        public string departure_time { get; set; }
        public string platform { get; set; }
        public int canceled { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
        public string SeeTime { get; set; }
    }
}
