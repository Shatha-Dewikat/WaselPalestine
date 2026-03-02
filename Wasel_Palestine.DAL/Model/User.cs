using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }

        public bool IsActive { get; set; }

        public bool EmailVerified { get; set; }

        public DateTime? EmailVerifiedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

      
        public List<UserRole> UserRoles { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public List<UserSession> UserSessions { get; set; }
        public List<AuditLog> AuditLogs { get; set; }
        public List<CheckpointStatusHistory> CheckpointStatusHistories { get; set; }
        public List<Incident> CreatedIncidents { get; set; }
        public List<Incident> VerifiedIncidents { get; set; }
        public List<Incident> ClosedIncidents { get; set; }
        public List<Report> Reports { get; set; }
        public List<ReportVote> ReportVotes { get; set; }
        public List<ReportModerationAction> ReportModerationActions { get; set; }
        public List<RouteRequest> RouteRequests { get; set; }
        public List<AlertRecipient> AlertRecipients { get; set; }
        public List<AlertSubscription> AlertSubscriptions { get; set; }
        public List<ReportMedia> ReportMedias { get; set; }
    }
}
