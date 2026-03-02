using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<User> Users { get; set; }
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

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ======================
            // Identity tables rename
            // ======================
            builder.Ignore<Microsoft.AspNetCore.Identity.IdentityPasskeyData>();
            builder.Entity<IdentityUser>().ToTable("IdentityUser");
            builder.Entity<IdentityRole>().ToTable("IdentityRole");
            builder.Entity<IdentityUserRole<string>>().ToTable("IdentityUserRole");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

        
            builder.Entity<Location>(entity =>
            {
                entity.Property(e => e.Latitude).HasPrecision(9, 6);
                entity.Property(e => e.Longitude).HasPrecision(9, 6);
            });

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

        
            builder.Entity<CheckpointStatusHistory>()
                .HasOne(csh => csh.ChangedByUser)
                .WithMany(u => u.CheckpointStatusHistories)
                .HasForeignKey(csh => csh.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

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

            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        
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

          
            builder.Entity<IncidentMedia>()
                .HasOne(im => im.Incident)
                .WithMany(i => i.IncidentMedia)
                .HasForeignKey(im => im.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

        
            builder.Entity<IncidentHistory>()
                .HasOne(ih => ih.Incident)
                .WithMany(i => i.IncidentHistories)
                .HasForeignKey(ih => ih.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<IncidentHistory>()
                .HasOne(ih => ih.Status)
                .WithMany(s => s.IncidentHistories)
                .HasForeignKey(ih => ih.StatusId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}