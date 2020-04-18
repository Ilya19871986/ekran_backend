using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.Models
{
    [Table("panels")]
    public class Panel
    {
        public int id { get; set; }
        public string panel_name { get; set; }
        public string last_connect { get; set; }
        public string player_version { get; set; }
        public int user_id { get; set; }
        public int run_text { get; set; }
        public int time_vip { get; set; }
        public string address { get; set; }
    }
}
