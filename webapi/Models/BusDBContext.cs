using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.Models
{
    public class BusDBContext : DbContext
    {
        public DbSet<Bus> Buses { get; set; }
        public BusDBContext(DbContextOptions<BusDBContext> options) : base(options)
        {

        }
    }
}
