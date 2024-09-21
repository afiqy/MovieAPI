using Microsoft.EntityFrameworkCore;
using MovieAPI.Models;

namespace MovieAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Favourite> Favourites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Favourite>()
                .HasKey(f => f.Id);

            modelBuilder.Entity<Favourite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favourites)
                .HasForeignKey(f => f.UserId)
                .IsRequired();

            modelBuilder.Entity<Favourite>()
                .HasOne(f => f.Movie)
                .WithMany(m => m.Favourites)
                .HasForeignKey(f => f.MovieId)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}
