using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Wasel_Palestine.BLL.MapsterConfigration;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;
using Wasel_Palestine.DAL.Utils;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Wasel_Palestine.PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddMemoryCache();
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

            builder.Services.AddRateLimiter(options =>
            {
               
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "You have exceeded the allowed order limit. Please try again later."
                    }, cancellationToken: token);
                };

               
                options.AddPolicy("fixed-by-ip", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100, // 100 طلب
                            Window = TimeSpan.FromMinutes(1), // كل دقيقة
                            QueueLimit = 0
                        }));

               
                options.AddPolicy("strict-by-ip", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5, // 5 طلبات فقط
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
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
            builder.Services.AddScoped<ICheckpointStatusService, CheckpointStatusService>();
            builder.Services.AddScoped<IIncidentService, IncidentService>();
            builder.Services.AddScoped<IIncidentCategoryService, IncidentCategoryService>();
            builder.Services.AddScoped<IIncidentSeverityService, IncidentSeverityService>();
            builder.Services.AddScoped<IIncidentStatusService, IncidentStatusService>();
            builder.Services.AddScoped<IIncidentMediaService, IncidentMediaService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<ICheckpointService, CheckpointService>();
            builder.Services.AddHttpClient<IWeatherService, WeatherService>();
            builder.Services.AddHostedService<WeatherBackgroundService>();
            // Repositories
            builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
            builder.Services.AddScoped<IIncidentCategoryRepository, IncidentCategoryRepository>();
            builder.Services.AddScoped<IIncidentHistoryRepository, IncidentHistoryRepository>();
            builder.Services.AddScoped<IIncidentSeverityRepository, IncidentSeverityRepository>();
            builder.Services.AddScoped<IIncidentStatusRepository, IncidentStatusRepository>();
            builder.Services.AddScoped<IIncidentMediaRepository, IncidentMediaRepository>();
            builder.Services.AddScoped<ICheckpointRepository, CheckpointRepository>();
            builder.Services.AddScoped<ICheckpointStatusRepository, CheckpointStatusRepository>();

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
            MapsterConfig.RegisterMappings();
            var app = builder.Build();
            app.UseRateLimiter();
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