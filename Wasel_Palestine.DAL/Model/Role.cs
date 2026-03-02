using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Wasel_Palestine.DAL.Model
{
    public class Role 
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<UserRole> UserRoles { get; set; }
    }
}
