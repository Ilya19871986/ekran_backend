using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi.Models
{
    [Table("users")]
    public class User
    {
        public int Id { get; set; }
        public string user_name { get; set; }
        public string password { get; set; }
        public int CountPanel { get; set; }
        public string Role { get; set; }
        public string working_folder { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string description { get; set; }
        public string AuthTime { get; set; }
        public bool locked { get; set; }
        public bool deleted { get; set; }
        public string organization { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string town { get; set; }
        public DateTime creation_date { get; set; }
        public int adminId { get; set; }
    }
}
