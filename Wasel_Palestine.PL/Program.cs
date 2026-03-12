using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;
using Wasel_Palestine.DAL.Utils;
using Mapster;

namespace Wasel_Palestine.PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ----------------- DbContext -----------------
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ----------------- Identity -----------------
            builder.Services.AddIdentity<User, Role>(options =>
            {
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ----------------- JWT -----------------
            var jwt = builder.Configuration.GetSection("Jwt");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.IncludeErrorDetails = true;

                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt["Issuer"],
                    ValidAudience = jwt["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt["SecretKey"]!)),
                    ClockSkew = TimeSpan.Zero
                };
            });

          
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
                options.AddPolicy("ModeratorOnly", p => p.RequireRole("Moderator"));
                options.AddPolicy("AdminOrModerator", p => p.RequireRole("Admin", "Moderator"));
                options.AddPolicy("ActiveUserOnly", p => p.RequireClaim("isActive", "true"));
            });

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter("ActiveUserOnly"));
            });

          
            builder.Services.AddHttpContextAccessor();

            // Services
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddTransient<IEmailSender, EmailSender>(); 

            builder.Services.AddScoped<IIncidentService, IncidentService>();
            builder.Services.AddScoped<IIncidentCategoryService, IncidentCategoryService>();
            builder.Services.AddScoped<IIncidentSeverityService, IncidentSeverityService>();
            builder.Services.AddScoped<IIncidentStatusService, IncidentStatusService>();
            builder.Services.AddScoped<IIncidentMediaService, IncidentMediaService>();
            builder.Services.AddScoped<IFileService, FileService>();

            // Repositories
            builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
            builder.Services.AddScoped<IIncidentCategoryRepository, IncidentCategoryRepository>();
            builder.Services.AddScoped<IIncidentHistoryRepository, IncidentHistoryRepository>();
            builder.Services.AddScoped<IIncidentSeverityRepository, IncidentSeverityRepository>();
            builder.Services.AddScoped<IIncidentStatusRepository, IncidentStatusRepository>();
            builder.Services.AddScoped<IIncidentMediaRepository, IncidentMediaRepository>();

            // Seeders
            builder.Services.AddScoped<RoleSeedData>();
            builder.Services.AddScoped<UserSeedData>();
            builder.Services.AddScoped<ReportStatusSeedData>();
            builder.Services.AddScoped<ISeedData, RoleSeedData>();
            builder.Services.AddScoped<ISeedData, UserSeedData>();
            builder.Services.AddScoped<ISeedData, ReportStatusSeedData>();

            // Utils
            builder.Services.AddScoped<AuditLogger>();
            builder.Services.AddMapster();
            builder.Services.AddOpenApi();

            // ----------------- Build & Run -----------------
            var app = builder.Build();

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            // Seed Data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var roleSeeder = services.GetRequiredService<RoleSeedData>();
                await roleSeeder.DataSeed();

                var userSeeder = services.GetRequiredService<UserSeedData>();
                await userSeeder.DataSeed();

                var statusSeeder = services.GetRequiredService<ReportStatusSeedData>();
                await statusSeeder.DataSeed();
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.MapControllers();
            app.Run();
        }
    }
}