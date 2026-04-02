using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
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
            // Admin
            await EnsureUserWithRoleAsync(
                userName: "DRabaya",
                email: "d@gmail.com",
                fullName: "Duha Rabaya",
                password: "Admin@123",
                roleName: "Admin"
            );

            // Supervisor
            await EnsureUserWithRoleAsync(
                userName: "Shatha_Dwikat",
                email: "sdwikat93@gmail.com",
                fullName: "Shatha Dwikat",
                password: "Admin@123",
                roleName: "Supervisor"
            );

            // Normal User
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

                    // ✅ تأكيد الإيميل من البداية للـ seed users
                    EmailConfirmed = true,
                    EmailVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var create = await _userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                    throw new Exception($"Create user failed: {string.Join(" | ", create.Errors.Select(e => e.Description))}");
            }
            else
            {
                // ✅ تأكد من القيم حتى لو اليوزر موجود من قبل
                var changed = false;

                if (!user.IsActive)
                {
                    user.IsActive = true;
                    changed = true;
                }

                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    changed = true;
                }

                if (!user.EmailVerified)
                {
                    user.EmailVerified = true;
                    changed = true;
                }

                // إذا CreatedAt فاضي (حسب موديلك)
                if (user.CreatedAt == default)
                {
                    user.CreatedAt = DateTime.UtcNow;
                    changed = true;
                }

                if (changed)
                    await _userManager.UpdateAsync(user);
            }

            // ✅ Ensure role assigned
            if (!await _userManager.IsInRoleAsync(user, roleName))
                await _userManager.AddToRoleAsync(user, roleName);

                var ok = await _userManager.CheckPasswordAsync(user, password);
if (!ok)
{
    // unlock/reset failed count (احتياط)
    await _userManager.SetLockoutEndDateAsync(user, null);
    await _userManager.ResetAccessFailedCountAsync(user);

    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
    var resetRes = await _userManager.ResetPasswordAsync(user, resetToken, password);

    if (!resetRes.Succeeded)
        throw new Exception($"Reset password failed for {email}: {string.Join(" | ", resetRes.Errors.Select(e => e.Description))}");
}
        }
    }
}