using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.Models
{
    public class PanelsDBContext : DbContext
    {
        public DbSet<Panel> Panels { get; set; }
        public DbSet<Content> Content { get; set; }
        public PanelsDBContext(DbContextOptions<PanelsDBContext> options) : base(options)
        {

        }
    }
}
