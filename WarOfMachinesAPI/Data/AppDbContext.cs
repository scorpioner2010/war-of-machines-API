using WarOfMachinesAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace WarOfMachinesAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Player> Players => Set<Player>();
    }
}