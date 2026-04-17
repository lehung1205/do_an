using JobPortal.API.Data;
using JobPortal.API.Repositories;
using JobPortal.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));
// // Identity
// builder.Services.AddIdentity<User, IdentityRole>()
//     .AddEntityFrameworkStores<AppDbContext>()
//     .AddDefaultTokenProviders();

// JWT Authentication
// var jwtKey = builder.Configuration["Jwt:Key"]!;
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],
//             ValidAudience = builder.Configuration["Jwt:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
//         };
//         // Để SignalR gửi token qua query string
//         options.Events = new JwtBearerEvents
//         {
//             OnMessageReceived = context =>
//             {
//                 var token = context.Request.Query["access_token"];
//                 if (!string.IsNullOrEmpty(token))
//                     context.Token = token;
//                 return Task.CompletedTask;
//             }
//         };
//     });

// Services
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();
// builder.Services.AddScoped<ChatBotService>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddSignalR();

// CORS cho Razor Pages gọi được API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowRazor", policy =>
        policy.WithOrigins("https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // cần cho SignalR
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// app.UseSwagger();
// app.UseSwaggerUI();
app.UseCors("AllowRazor");
app.UseAuthentication();
app.UseAuthorization();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    if (!context.Jobs.Any())
    {
        context.Jobs.AddRange(
            new JobPortal.API.Models.Job { Title = "Software Engineer", Company = "Google", Location = "Mountain View, CA", Salary = 150000, Description = "Full stack developer" },
            new JobPortal.API.Models.Job { Title = "Data Scientist", Company = "Facebook", Location = "Menlo Park, CA", Salary = 160000, Description = "Machine learning expert" },
            new JobPortal.API.Models.Job { Title = "Product Manager", Company = "Amazon", Location = "Seattle, WA", Salary = 140000, Description = "Product owner" }
        );
        context.SaveChanges();
    }
}

app.MapControllers();
// app.MapHub<ChatHub>("/hubs/chat");

app.Run();

