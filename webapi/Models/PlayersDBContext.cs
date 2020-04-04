using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace webapi.Models
{
    public class PlayersDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public PlayersDBContext(DbContextOptions<PlayersDBContext> options) : base(options)
        {

        }
    }
}
