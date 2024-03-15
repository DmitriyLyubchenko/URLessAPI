using Microsoft.EntityFrameworkCore;
using URLessDAL.Entities;

namespace URLessDAL.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Url> Urls { get; set; }

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder
                .Entity<Url>()
                .HasKey(x => x.Id);
        }
    }
}
