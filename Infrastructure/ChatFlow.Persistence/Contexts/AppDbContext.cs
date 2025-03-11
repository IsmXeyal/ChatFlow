using ChatFlow.Domain.Entities.Concretes;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ChatFlow.Persistence.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }
    public virtual DbSet<EmailConfirmToken> EmailConfirmTokens { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<RePasswordToken> RePasswordTokens { get; set; }
    public virtual DbSet<Token> Tokens { get; set; }
    public virtual DbSet<UserAgent> UserAgents { get; set; }
}