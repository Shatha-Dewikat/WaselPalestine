using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wasel_Palestine.DAL.Model;

namespace Wasel_Palestine.DAL.Utils
{
    public class UserSeedData : ISeedData
    {
        private readonly UserManager<User> _userManager;

        public UserSeedData(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task DataSeed()
        {
           
            await EnsureUserWithRoleAsync(
                userName: "DRabaya",
                email: "d@gmail.com",
                fullName: "Duha Rabaya",
                password: "Admin@123",
                roleName: "Admin"
            );

          
            await EnsureUserWithRoleAsync(
                userName: "Shatha_Dwikat",
                email: "sdwikat93@gmail.com",
                fullName: "Shatha Dwikat",
                password: "Admin@123",
                roleName: "Moderator"
            );

            await EnsureUserWithRoleAsync(
                userName: "Abed",
                email: "a@gmail.com",
                fullName: "Abed Edaily",
                password: "Admin@123",
                roleName: "User"
            );
        }

        private async Task EnsureUserWithRoleAsync(string userName, string email, string fullName, string password, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    UserName = userName,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,

                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                };

                var create = await _userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                    throw new Exception($"Create user failed: {string.Join(" | ", create.Errors.Select(e => e.Description))}");
            }
            else
            {
              
                if (!user.IsActive)
                {
                    user.IsActive = true;
                    await _userManager.UpdateAsync(user);
                }
            }

         
            if (!await _userManager.IsInRoleAsync(user, roleName))
                await _userManager.AddToRoleAsync(user, roleName);
        }
    }
}