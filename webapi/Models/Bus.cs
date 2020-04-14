using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.Models
{
    [Table("schedule_bus")]
    public class Bus
    {
        public int Id { get; set; }
        public string from_ { get; set; }
        public string to_ { get; set; }
        public TimeSpan departure_time { get; set; }
        public int platform { get; set; }
        public int canceled { get; set; }
        public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }
        public int Saturday { get; set; }
        public int Sunday { get; set; }
        public int SeeTime { get; set; }
    }
}
