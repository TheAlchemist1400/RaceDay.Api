using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    // SQL Tables
    public DbSet<EventOrganizer> EventOrganizers => Set<EventOrganizer>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<EventLocation> EventLocations => Set<EventLocation>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<Weather> WeatherRecords => Set<Weather>();
    public DbSet<RouteInfo> RouteInfos => Set<RouteInfo>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ======================================== EVENT ORGANIZER ========================================
        modelBuilder.Entity<EventOrganizer>(entity =>
        {
            entity.ToTable("EventOrganizer");
            entity.HasIndex(organizer => organizer.Email).IsUnique();
        });

        // ======================================== PARTICIPANT ========================================
        modelBuilder.Entity<Participant>(entity =>
        {
            entity.ToTable("Participant");
            entity.HasIndex(participant => participant.Email).IsUnique();
        });

        modelBuilder.Entity<EventLocation>().ToTable("EventLocation");

        // ======================================== EVENT ========================================
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Event");
            entity.Property(eventItem => eventItem.EventDate).HasColumnType("date");

            // An event belongs to one organiser and one location.
            entity.HasOne(eventItem => eventItem.Organizer)
                .WithMany()
                .HasForeignKey(eventItem => eventItem.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(eventItem => eventItem.Location)
                .WithMany()
                .HasForeignKey(eventItem => eventItem.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ======================================== ACTIVITY ========================================
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("Activity");
            entity.Property(activity => activity.DistanceKm).HasPrecision(6, 2);
            entity.Property(activity => activity.EntryFee).HasPrecision(10, 2);
            entity.Property(activity => activity.StartTime).HasColumnType("time(0)");

            // An event cannot contain two activities with the same name.
            entity.HasIndex(activity => new { activity.EventId, activity.ActivityName })
                .IsUnique();

            entity.HasAlternateKey(activity => new { activity.EventId, activity.ActivityId });

            entity.HasOne(activity => activity.Event)
                .WithMany()
                .HasForeignKey(activity => activity.EventId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ======================================== REGISTRATION ========================================
        modelBuilder.Entity<Registration>(entity =>
        {
            entity.ToTable("Registration");
            entity.Property(registration => registration.RegistrationDate)
                .HasColumnType("date")
                .HasDefaultValueSql("CONVERT(date, GETDATE())");

            entity.Property(registration => registration.RegistrationStatus)
                .HasDefaultValue("Confirmed");

            // A participant may register only once for a specific event.
            entity.HasIndex(registration => new
            {
                registration.ParticipantId,
                registration.EventId
            }).IsUnique();

            // Bib numbers are unique within an event, but can be unassigned.
            entity.HasIndex(registration => new
            {
                registration.EventId,
                registration.BibNumber
            }).IsUnique()
              .HasFilter("[BibNumber] IS NOT NULL");

            entity.HasOne(registration => registration.Participant)
                .WithMany()
                .HasForeignKey(registration => registration.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(registration => registration.Event)
                .WithMany()
                .HasForeignKey(registration => registration.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(registration => registration.Activity)
                .WithMany()
                .HasForeignKey(registration => new
                {
                    registration.EventId,
                    registration.ActivityId
                })
                .HasPrincipalKey(activity => new
                {
                    activity.EventId,
                    activity.ActivityId
                })
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ======================================== RESULT ========================================
        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("Result");
            entity.Property(result => result.FinishTime).HasColumnType("time(0)");

            // One registration can produce at most one result.
            entity.HasIndex(result => result.RegistrationId).IsUnique();

            entity.HasOne(result => result.Registration)
                .WithOne()
                .HasForeignKey<Result>(result => result.RegistrationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ======================================== WEATHER ========================================
        modelBuilder.Entity<Weather>(entity =>
        {
            entity.ToTable("Weather");
            entity.Property(weather => weather.Temperature).HasPrecision(4, 1);
            entity.Property(weather => weather.WindSpeed).HasPrecision(5, 1);
            entity.HasIndex(weather => weather.EventId).IsUnique();

            entity.HasOne(weather => weather.Event)
                .WithOne()
                .HasForeignKey<Weather>(weather => weather.EventId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ======================================== ROUTE INFO ========================================
        modelBuilder.Entity<RouteInfo>(entity =>
        {
            entity.ToTable("RouteInfo");
            entity.Property(route => route.DistanceKm).HasPrecision(5, 2);
            entity.HasIndex(route => route.EventId).IsUnique();

            entity.HasOne(route => route.Event)
                .WithOne()
                .HasForeignKey<RouteInfo>(route => route.EventId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}