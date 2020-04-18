using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.Models
{
    [Table("content")]
    public class Content
    {
        public int Id { get; set; }
        public string file_name { get; set; }
        public int file_size { get; set; }
        public int sync { get; set; }
        public int deleted { get; set; }
        public DateTime end_date { get; set; }
        public int user_id { get; set; }
        public int panel_id { get; set; }
        public int type_content { get; set; }
    }
}
