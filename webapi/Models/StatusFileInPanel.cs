using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.Models
{
    public class StatusFileInPanel
    {
        public StatusFileInPanel(string panel_name, int sync)
        {
            this.panel_name = panel_name;
            this.sync = sync;
        }

        public string panel_name { get; set; }
        public int sync { get; set; }
    }
}
