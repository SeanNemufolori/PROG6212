using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Enrolment> Enrolments { get; set; }
    public DbSet<Result> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraints
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Enrolment>().HasIndex(e => new { e.EventId, e.ParticipantId }).IsUnique();
        modelBuilder.Entity<Result>().HasIndex(r => r.EnrolmentId).IsUnique();

        // Relationships
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(u => u.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Categories)
            .WithOne(c => c.Event)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Enrolments)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Enrolments)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Participant)
            .WithMany(u => u.Enrolments)
            .HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Result)
            .WithOne(r => r.Enrolment)
            .HasForeignKey<Result>(r => r.EnrolmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.CapturedByUser)
            .WithMany(u => u.CapturedResults)
            .HasForeignKey(r => r.CapturedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}