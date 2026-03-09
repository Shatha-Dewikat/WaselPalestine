using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

            // DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity + Lockout (Brute-force protection)
            builder.Services.AddIdentity<User, Role>(options =>
            {
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // JWT Authentication
            var jwt = builder.Configuration.GetSection("Jwt");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt["Issuer"],
                        ValidAudience = jwt["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
                options.AddPolicy("ActiveUserOnly", p => p.RequireClaim("isActive", "true"));
            });

           builder.Services.AddControllers()
    .AddApplicationPart(typeof(Wasel_Palestine.PL.Program).Assembly);

            // Seeders
            builder.Services.AddScoped<RoleSeedData>();
            builder.Services.AddScoped<UserSeedData>();
            builder.Services.AddScoped<ReportStatusSeedData>();

            // Utilities (بنضيفهم هسا عشان auth يشتغل)
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<AuditLogger>();

            // OpenAPI
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseStaticFiles();
           // app.UseHttpsRedirection();

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