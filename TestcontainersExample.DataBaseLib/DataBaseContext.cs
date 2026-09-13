using Microsoft.EntityFrameworkCore;

namespace TestcontainersExample.DataBaseLib;

public class DataBaseContext : DbContext
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }

    public DataBaseContext(DbContextOptions<DataBaseContext> options) 
        : base(options) 
    { }
}