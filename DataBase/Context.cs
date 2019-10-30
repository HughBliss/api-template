using Microsoft.EntityFrameworkCore;
using Study.Models;

namespace Study.DataBase
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }
        public DbSet<Order> Orders { get; set; }
    }
}