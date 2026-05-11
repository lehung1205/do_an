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
builder.Services.AddScoped<ICongViecRepository, CongViecRepository>();
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

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// app.UseSwagger();
// app.UseSwaggerUI();
app.UseCors("AllowRazor");
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    if (!context.CongViecs.Any())
    {
        var danhMuc = new JobPortal.API.Models.DanhMuc { Ten = "Công nghệ thông tin" };
        context.DanhMucs.Add(danhMuc);
        context.SaveChanges();

        var ntd = new JobPortal.API.Models.NhaTuyenDung
        {
            Ten = "Công ty mẫu",
            Email = "contact@mau.vn",
            MatKhau = "changeme",
            TrangThai = "hoat_dong",
            SoLuotBaiDang = 10
        };
        context.NhaTuyenDungs.Add(ntd);
        context.SaveChanges();

        var now = DateTime.UtcNow;
        context.CongViecs.AddRange(
            new JobPortal.API.Models.CongViec
            {
                IdTuyenDung = ntd.IdTuyenDung,
                IdDanhMuc = danhMuc.IdDanhMuc,
                TieuDe = "Lập trình viên Backend",
                MoTa = "Phát triển API, làm việc với .NET và MySQL.",
                MucLuong = 25_000_000,
                DiaDiem = "TP.HCM",
                TrangThaiBaiDang = "dang_tuyen",
                NgayBatDau = now,
                NgayKetThuc = now.AddMonths(3),
                NgayHetHan = now.AddMonths(2)
            },
            new JobPortal.API.Models.CongViec
            {
                IdTuyenDung = ntd.IdTuyenDung,
                IdDanhMuc = danhMuc.IdDanhMuc,
                TieuDe = "Kỹ sư dữ liệu",
                MoTa = "Phân tích dữ liệu, xây dựng mô hình ML.",
                MucLuong = 30_000_000,
                DiaDiem = "Hà Nội",
                TrangThaiBaiDang = "dang_tuyen",
                NgayBatDau = now,
                NgayKetThuc = now.AddMonths(3),
                NgayHetHan = now.AddMonths(2)
            });
        context.SaveChanges();
    }
}

app.MapControllers();
// app.MapHub<ChatHub>("/hubs/chat");

app.Run();

