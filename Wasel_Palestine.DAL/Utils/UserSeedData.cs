using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
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
            if (!await _userManager.Users.AnyAsync())
            {
                var user1 = new User
                {
                    UserName = "Shatha_Dwikat",
                    Email = "sdwikat93@gmail.com",
                    FullName = "Shatha Dwikat",
                    EmailConfirmed = true
                };

                var user2 = new User
                {
                    UserName = "DRabaya",
                    Email = "d@gmail.com",
                    FullName = "Duha Rabaya",
                    EmailConfirmed = true
                };

                var user3 = new User
                {
                    UserName = "Abed",
                    Email = "a@gmail.com",
                    FullName = "Abed Edaily",
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(user1, "Admin@123");
                await _userManager.CreateAsync(user2, "Admin@123");
                await _userManager.CreateAsync(user3, "Admin@123");

                await _userManager.AddToRoleAsync(user1, "Supervisor");
                await _userManager.AddToRoleAsync(user2, "Admin");
                await _userManager.AddToRoleAsync(user3, "User");
            }
        }
    }
}
