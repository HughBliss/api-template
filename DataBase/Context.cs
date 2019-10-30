using Microsoft.EntityFrameworkCore;

namespace Study.DataBase
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }
    }
}