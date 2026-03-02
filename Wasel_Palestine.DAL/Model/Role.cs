using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class Role : IdentityRole
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<UserRole> UserRoles { get; set; }
    }
}
