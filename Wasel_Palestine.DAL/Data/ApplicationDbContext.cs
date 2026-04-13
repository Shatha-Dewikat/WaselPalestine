using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<CityIncidentStats> CityIncidentStats { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<IncidentCategory> IncidentCategories { get; set; }
        public DbSet<IncidentSeverity> IncidentSeverities { get; set; }
        public DbSet<IncidentStatus> IncidentStatuses { get; set; }
        public DbSet<ReportStatus> ReportStatuses { get; set; }
        public DbSet<Checkpoint> Checkpoints { get; set; }
        public DbSet<CheckpointStatusHistory> CheckpointStatusHistories { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<IncidentHistory> IncidentHistories { get; set; }
        public DbSet<IncidentMedia> IncidentMedias { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportVote> ReportVotes { get; set; }
        public DbSet<ReportMedia> ReportMedias { get; set; }
        public DbSet<ReportModerationAction> ReportModerationActions { get; set; }
        public DbSet<RouteRequest> RouteRequests { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<AlertRecipient> AlertRecipients { get; set; }
        public DbSet<AlertSubscription> AlertSubscriptions { get; set; }
        public DbSet<AlertHistory> AlertHistories { get; set; }
        public DbSet<ExternalApiCache> ExternalApiCaches { get; set; }
        public DbSet<CheckpointStatus> CheckpointStatuses { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Identity tables
            builder.Entity<User>().ToTable("AspNetUsers");
            builder.Entity<Role>().ToTable("AspNetRoles");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");

            // Location
            builder.Entity<Location>(entity =>
            {
                entity.Property(e => e.Latitude).HasPrecision(9, 6).IsRequired();
                entity.Property(e => e.Longitude).HasPrecision(9, 6).IsRequired();
                entity.Property(e => e.AreaName).HasMaxLength(100);
                entity.Property(e => e.City).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // AuditLogs
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.UserId).IsRequired(false);
                entity.Property(a => a.Action).HasMaxLength(50).IsRequired();
                entity.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Details).HasMaxLength(500);
                entity.Property(a => a.IPAddress).HasMaxLength(45);
                entity.Property(a => a.UserAgent).HasMaxLength(256);
                entity.Property(a => a.Timestamp).HasDefaultValueSql("GETUTCDATE()").IsRequired();
            });

            // IncidentHistory
            builder.Entity<IncidentHistory>(entity =>
            {
                entity.HasKey(ih => ih.Id);
                entity.Property(ih => ih.Action).HasMaxLength(50).IsRequired();
                entity.Property(ih => ih.Changes).HasMaxLength(500);

                entity.HasOne(ih => ih.Incident)
                      .WithMany(i => i.IncidentHistories)
                      .HasForeignKey(ih => ih.IncidentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ih => ih.Status)
                      .WithMany(s => s.IncidentHistories)
                      .HasForeignKey(ih => ih.StatusId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // UserRole composite key
            builder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Incident relationships
            builder.Entity<Incident>()
                .HasOne(i => i.CreatedByUser)
                .WithMany(u => u.CreatedIncidents)
                .HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Incident>()
                .HasOne(i => i.VerifiedByUser)
                .WithMany(u => u.VerifiedIncidents)
                .HasForeignKey(i => i.VerifiedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Incident>()
                .HasOne(i => i.ClosedByUser)
                .WithMany(u => u.ClosedIncidents)
                .HasForeignKey(i => i.ClosedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Incident Media
            builder.Entity<IncidentMedia>()
                .HasOne(im => im.Incident)
                .WithMany(i => i.IncidentMedia)
                .HasForeignKey(im => im.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reports
            builder.Entity<ReportMedia>()
                .HasOne(rm => rm.User)
                .WithMany(u => u.ReportMedias)
                .HasForeignKey(rm => rm.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ReportMedia>()
                .HasOne(rm => rm.Report)
                .WithMany(r => r.ReportMedias)
                .HasForeignKey(rm => rm.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReportVote>()
                .HasOne(rv => rv.User)
                .WithMany(u => u.ReportVotes)
                .HasForeignKey(rv => rv.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ReportVote>()
                .HasOne(rv => rv.Report)
                .WithMany(r => r.ReportVotes)
                .HasForeignKey(rv => rv.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReportModerationAction>()
                .HasOne(rm => rm.Moderator)
                .WithMany(u => u.ReportModerationActions)
                .HasForeignKey(rm => rm.ModeratorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ReportModerationAction>()
                .HasOne(rm => rm.Report)
                .WithMany(r => r.ReportModerationActions)
                .HasForeignKey(rm => rm.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Checkpoints
            builder.Entity<Checkpoint>()
                .HasOne(c => c.Location)
                .WithMany(l => l.Checkpoints)
                .HasForeignKey(c => c.LocationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Checkpoint>()
                .HasMany(c => c.StatusHistories)
                .WithOne(csh => csh.Checkpoint)
                .HasForeignKey(csh => csh.CheckpointId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CheckpointStatusHistory>()
                .HasOne(csh => csh.ChangedByUser)
                .WithMany(u => u.CheckpointStatusHistories)
                .HasForeignKey(csh => csh.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Alerts
            builder.Entity<AlertRecipient>()
                .HasOne(ar => ar.User)
                .WithMany(u => u.AlertRecipients)
                .HasForeignKey(ar => ar.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AlertRecipient>()
                .HasOne(ar => ar.Alert)
                .WithMany(a => a.Recipients)
                .HasForeignKey(ar => ar.AlertId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AlertHistory>()
                .HasOne(ah => ah.Alert)
                .WithMany(a => a.AlertHistories)
                .HasForeignKey(ah => ah.AlertId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AlertSubscription>()
                .HasOne(asub => asub.User)
                .WithMany(u => u.AlertSubscriptions)
                .HasForeignKey(asub => asub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AlertSubscription>()
                .HasOne(asub => asub.Location)
                .WithMany()
                .HasForeignKey(asub => asub.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AlertSubscription>()
                .HasOne(asub => asub.Category)
                .WithMany(c => c.AlertSubscriptions)
                .HasForeignKey(asub => asub.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // RouteRequests
            builder.Entity<RouteRequest>()
                .HasOne(rr => rr.User)
                .WithMany(u => u.RouteRequests)
                .HasForeignKey(rr => rr.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<RouteRequest>()
                .HasOne(rr => rr.FromLocation)
                .WithMany(l => l.FromRouteRequests)
                .HasForeignKey(rr => rr.FromLocationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<RouteRequest>()
                .HasOne(rr => rr.ToLocation)
                .WithMany(l => l.ToRouteRequests)
                .HasForeignKey(rr => rr.ToLocationId)
                .OnDelete(DeleteBehavior.NoAction);

            // RefreshTokens
            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Incident Categories, Severities, Statuses
            builder.Entity<Incident>()
                .HasOne(i => i.Category)
                .WithMany()
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Incident>()
                .HasOne(i => i.Severity)
                .WithMany()
                .HasForeignKey(i => i.SeverityId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Incident>()
                .HasOne(i => i.Status)
                .WithMany()
                .HasForeignKey(i => i.StatusId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Incident>()
    .HasOne(i => i.RelatedCheckpoint) 
    .WithMany(c => c.RelatedIncidents) 
    .HasForeignKey(i => i.RelatedCheckpointId)
    .OnDelete(DeleteBehavior.SetNull);

            

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.HasIndex(rt => rt.UserId);
            });

            builder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(a => a.Timestamp);
                entity.HasIndex(a => a.UserId);
            });

         
            builder.Entity<Location>(entity =>
            {
                entity.HasIndex(l => new { l.Latitude, l.Longitude });
                entity.HasIndex(l => l.City);
            });

            builder.Entity<Incident>(entity =>
            {
                entity.HasIndex(i => i.StatusId);
                entity.HasIndex(i => i.CreatedAt);
            });


        }


    }
}