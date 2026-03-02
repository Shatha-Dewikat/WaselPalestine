using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Utils
{
    public class RoleSeedData : ISeedData
    {
        private readonly RoleManager<Role> _roleManager;

        public RoleSeedData(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task DataSeed()
        {
            string[] roles = { "Supervisor", "Admin", "User" };

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new Role
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await _roleManager.CreateAsync(role);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"Error creating role {roleName}: {error.Description}");
                        }
                    }
                }
            }
        }
    }
}