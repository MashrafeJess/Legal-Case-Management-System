using Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Database.Context
{
    public class LMSContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                @"Host=localhost;Port=5432;Database=LMS;Username=postgres;Password=22345;",
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
    }
}