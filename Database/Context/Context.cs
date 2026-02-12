using Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Database.Context
{
    public class LMSContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=LMS;Username=postgres;Password=22345;",
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure());
        }

        public DbSet<SmtpSettings> SmtpSettings { get; set; }
        public DbSet<Token> Token { get; set; }
        public DbSet<Case> Case { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<Hearing> Hearing { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<PaymentMethod> PaymentMethod { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<CaseType> CaseType { get; set; }
        public DbSet<FileEntity> FileEntity { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
            new Role
            {
                RoleId = 1,
                RoleName = "Admin",
                CreatedBy = null,
                UpdatedBy = null,
                CreatedDate = new DateTime(2024, 2, 11, 0, 0, 0, DateTimeKind.Utc),
                UpdatedDate = null
            },
            new Role
            {
                RoleId = 2,
                RoleName = "Lawyer",
                CreatedBy = null,
                UpdatedBy = null,
                CreatedDate = new DateTime(2024, 2, 11, 0, 0, 0, DateTimeKind.Utc),
                UpdatedDate = null
            },
            new Role
            {
                RoleId = 3,
                RoleName = "Client",
                CreatedBy = null,
                UpdatedBy = null,
                CreatedDate = new DateTime(2024, 2, 11, 0, 0, 0, DateTimeKind.Utc),
                UpdatedDate = null
            }
        );
        }
    }
}