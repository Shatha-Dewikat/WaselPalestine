
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Utils;

namespace Wasel_Palestine.PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            builder.Services.AddControllers();

            builder.Services.AddScoped<RoleSeedData>();
            builder.Services.AddScoped<UserSeedData>();
            builder.Services.AddScoped<ReportStatusSeedData>();

            builder.Services.AddScoped<ISeedData, RoleSeedData>();
            builder.Services.AddScoped<ISeedData, UserSeedData>();
            builder.Services.AddScoped<ISeedData, ReportStatusSeedData>();




            builder.Services.AddOpenApi();

            var app = builder.Build();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                // أولاً نضيف Roles
                var roleSeeder = services.GetRequiredService<RoleSeedData>();
                await roleSeeder.DataSeed();

                // بعدين نضيف Users
                var userSeeder = services.GetRequiredService<UserSeedData>();
                await userSeeder.DataSeed();

                // وأخيراً أي Seeder آخر مثل ReportStatus
                var statusSeeder = services.GetRequiredService<ReportStatusSeedData>();
                await statusSeeder.DataSeed();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
