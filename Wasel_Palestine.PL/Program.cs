using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            
            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<RoleSeedData>();
            builder.Services.AddScoped<UserSeedData>();
            builder.Services.AddScoped<ReportStatusSeedData>();

            builder.Services.AddScoped<ISeedData, RoleSeedData>();
            builder.Services.AddScoped<ISeedData, UserSeedData>();
            builder.Services.AddScoped<ISeedData, ReportStatusSeedData>();
            
            builder.Services.AddScoped<IIncidentService, IncidentService>();

            builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
            builder.Services.AddScoped<IIncidentCategoryService, IncidentCategoryService>();
            builder.Services.AddScoped<IIncidentCategoryRepository, IncidentCategoryRepository>();

            builder.Services.AddMapster();
            builder.Services.AddOpenApi();

            var app = builder.Build();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
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

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
