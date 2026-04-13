using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Microsoft.AspNetCore.OpenApi;
using System.Text;
using System.Threading.RateLimiting;
using Wasel_Palestine.BAL.Service;
using Wasel_Palestine.BLL.MapsterConfigration;
using Wasel_Palestine.BLL.Service;
using Wasel_Palestine.DAL.Data;
using Wasel_Palestine.DAL.Model;
using Wasel_Palestine.DAL.Repository;
using Wasel_Palestine.DAL.Utils;
using Microsoft.OpenApi;



namespace Wasel_Palestine.PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddMemoryCache();
            builder.Services.AddResponseCompression();
            // DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    x => x.UseNetTopologySuite()
                ));

            // Identity
            builder.Services.AddIdentity<User, Role>(options =>
            {
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // JWT
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

            // Rate Limiting
            builder.Services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Too many requests"
                    }, cancellationToken: token);
                };

                options.AddPolicy("fixed-by-ip", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddPolicy("strict-by-ip", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });

            // Authorization (من شغلك)
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
                options.AddPolicy("ModeratorOnly", p => p.RequireRole("Moderator"));
                options.AddPolicy("AdminOrModerator", p => p.RequireRole("Admin", "Moderator"));
                options.AddPolicy("ActiveUserOnly", p => p.RequireClaim("isActive", "true"));
            });

            // Controllers + Services (دمج الاثنين)
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter("ActiveUserOnly"));
            });

            builder.Services.AddHttpContextAccessor();

           
           
            builder.Services.AddScoped<MobilityService>();
            builder.Services.AddScoped<ReportingService>();

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
            builder.Services.AddScoped<IAlertService, AlertService>();

            // Repositories
            builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
            builder.Services.AddScoped<IIncidentCategoryRepository, IncidentCategoryRepository>();
            builder.Services.AddScoped<IIncidentHistoryRepository, IncidentHistoryRepository>();
            builder.Services.AddScoped<IIncidentSeverityRepository, IncidentSeverityRepository>();
            builder.Services.AddScoped<IIncidentStatusRepository, IncidentStatusRepository>();
            builder.Services.AddScoped<IIncidentMediaRepository, IncidentMediaRepository>();
            builder.Services.AddScoped<ICheckpointRepository, CheckpointRepository>();
            builder.Services.AddScoped<ICheckpointStatusRepository, CheckpointStatusRepository>();
            builder.Services.AddScoped<IAlertRepository, AlertRepository>();

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
          //  builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();

            //builder.Services.AddSwaggerGen();

            builder.Services.AddAuthorization();

            // New Security API for Swashbuckle 10.x
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My Web API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme 
                {
                    Name = "Authorization", 
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", 
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Please enter token without Bearer"
                });

                options.AddSecurityRequirement(document =>
                    new OpenApiSecurityRequirement
                    {
                       
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
            });

            MapsterConfig.RegisterMappings();

            var app = builder.Build();
            app.UseSwagger(options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
            });



            app.UseRateLimiter();
            app.UseStaticFiles();
            app.UseAuthentication(); 
            app.UseAuthorization();


            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
               context.Database.Migrate(); 
              //  context.Database.EnsureCreated();
            }
            if (app.Environment.IsDevelopment())
            {
               // app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseResponseCompression();
            app.MapControllers();
            app.Run();
        }
    }
}