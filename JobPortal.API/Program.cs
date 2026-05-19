using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using JobPortal.API.Configurations;
using JobPortal.API.Data;
using JobPortal.API.Middleware;
using JobPortal.API.Models;
using JobPortal.API.Models.Auth;
using JobPortal.API.Repositories;
using JobPortal.API.Services;
using JobPortal.API.Services.Auth;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthLimiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });
});

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT configuration section is missing. Ensure Jwt settings are defined in appsettings.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException("JWT secret key is not configured. Set Jwt:Key in appsettings or environment variables.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = signingKey
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ADMIN", policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("EMPLOYER", policy => policy.RequireRole("EMPLOYER"));
    options.AddPolicy("JOB_SEEKER", policy => policy.RequireRole("JOB_SEEKER"));
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "JobPortal API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRateLimiter();
app.UseCors("AllowRazor");
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    var passwordHasher = new PasswordHasher<User>();

    if (!context.Users.Any())
    {
        var admin = new User
        {
            Name = "Administrator",
            Email = "admin@jobportal.local",
            Role = "ADMIN",
            PasswordHash = passwordHasher.HashPassword(null!, "Admin123!"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();

        context.Admins.Add(new Admin
        {
            Name = admin.Name,
            Email = admin.Email,
            PasswordHash = admin.PasswordHash,
            Status = "ACTIVE",
            Role = admin.Role,
            UserId = admin.Id,
            CreatedAt = admin.CreatedAt,
            UpdatedAt = admin.UpdatedAt
        });

        await context.SaveChangesAsync();
    }

    if (!context.Jobs.Any())
    {
        var category = new JobPortal.API.Models.Category { Name = "Information Technology" };
        context.Categories.Add(category);
        context.SaveChanges();

        var employerUser = new User
        {
            Name = "Sample Company",
            Email = "contact@sample.vn",
            Role = "EMPLOYER",
            PasswordHash = passwordHasher.HashPassword(null!, "Sample123!"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Users.Add(employerUser);
        context.SaveChanges();

        var employer = new JobPortal.API.Models.Employer
        {
            Name = "Sample Company",
            Email = "contact@sample.vn",
            PasswordHash = employerUser.PasswordHash,
            Status = "ACTIVE",
            Role = "EMPLOYER",
            PostingLimit = 10,
            UserId = employerUser.Id,
            CreatedAt = employerUser.CreatedAt,
            UpdatedAt = employerUser.UpdatedAt
        };
        context.Employers.Add(employer);
        context.SaveChanges();

        var now = DateTime.UtcNow;
        context.Jobs.AddRange(
            new JobPortal.API.Models.Job
            {
                EmployerId = employer.Id,
                CategoryId = category.Id,
                Title = "Backend Developer",
                Description = "Develop APIs, work with .NET and MySQL.",
                Salary = 25_000_000,
                Location = "Ho Chi Minh City",
                PostingStatus = "recruiting",
                StartDate = now,
                EndDate = now.AddMonths(3),
                ExpiryDate = now.AddMonths(2)
            },
            new JobPortal.API.Models.Job
            {
                EmployerId = employer.Id,
                CategoryId = category.Id,
                Title = "Data Engineer",
                Description = "Analyze data, build ML models.",
                Salary = 30_000_000,
                Location = "Hanoi",
                PostingStatus = "recruiting",
                StartDate = now,
                EndDate = now.AddMonths(3),
                ExpiryDate = now.AddMonths(2)
            });
        context.SaveChanges();
    }
}

app.MapControllers();

app.Run();

