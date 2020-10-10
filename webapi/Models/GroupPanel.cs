using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.Models
{
    [Table("group_panels")]
    public class GroupPanel
    {
        public int Id { get; set; }
        public string group_name { get; set; }
        public int user_id { get; set; }
        public string comment { get; set; }
        public int count_panel { get; set; }
    }
}
