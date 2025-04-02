namespace MailSenderAPI.Context;

using global::MailSenderAPI.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    public DbSet<SmtpSettings> SmtpSettings { get; set; }
    public DbSet<EmailExtension> EmailExtensions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailExtension>()
            .HasIndex(e => e.Extension)
            .IsUnique(); 

        base.OnModelCreating(modelBuilder);
    }
}
