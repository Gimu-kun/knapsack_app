using System;
using System.Text;
using knapsack_app.Pages.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using knapsack_app.Services; 
using Microsoft.AspNetCore.Authentication.Cookies; // Thêm thư viện này
using Microsoft.AspNetCore.Authentication; // Thêm thư viện này

var builder = WebApplication.CreateBuilder(args);

// Lấy BaseUrl từ cấu hình ApiSettings
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5238/";
Console.WriteLine($"[DEBUG] BaseUrl API: {apiBaseUrl}");

// Lấy JwtSettings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? 
    throw new InvalidOperationException("JwtSettings section is missing or invalid.");


// =========================================================
// 1. CẤU HÌNH DỊCH VỤ (SERVICE CONFIGURATION)
// =========================================================

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("*") // Cần thay thế "*" bằng domain frontend thực tế trong môi trường Production
            .AllowAnyHeader()
            .AllowAnyMethod();
            // Lưu ý: Không thể sử dụng AllowAnyOrigin() và AllowCredentials() cùng lúc với wildcard (*). 
            // Nếu bạn cần AllowCredentials, phải thay thế "*" bằng danh sách domain cụ thể.
            // Tôi đã bỏ .AllowCredentials() ở đây để tránh lỗi cấu hình CORS khi dùng wildcard.
    });
});

// Cấu hình JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddHttpClient();

// Cấu hình Database Context (MySQL)
builder.Services.AddDbContext<AppDbContext>(option =>
    option.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Cấu hình Controller và JSON options
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.IncludeFields = true;
    });

// Đăng ký Application Services (Scoped Services)
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ChallengeService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<LeaderBoardService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<AnalyzingService>();
builder.Services.AddScoped<UserProgressService>();
builder.Services.AddScoped<SessionService>();

// --- CẤU HÌNH AUTHENTICATION MỚI: Hỗ trợ Cookie-JWT Hybrid ---
builder.Services
    .AddAuthentication(options =>
    {
        // Đặt scheme mặc định cho tất cả các request (ví dụ: Razor Pages)
        options.DefaultAuthenticateScheme = "AdminCookieScheme"; 
        options.DefaultChallengeScheme = "AdminCookieScheme";
        options.DefaultSignInScheme = "AdminCookieScheme";
    })
    // 1. Cấu hình Scheme tùy chỉnh để đọc Token từ Cookie
    .AddCookie("AdminCookieScheme", options =>
    {
        options.Cookie.Name = "AdminToken";
        options.LoginPath = "/Admin/Login"; // Trang chuyển hướng khi không được cấp quyền
        options.AccessDeniedPath = "/Admin/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(6);
        options.SlidingExpiration = true;

        // Sự kiện quan trọng: Đọc token từ Cookie và xác thực JWT
        options.Events.OnValidatePrincipal = async context =>
        {
            var token = context.Request.Cookies["AdminToken"];

            if (!string.IsNullOrEmpty(token))
            {
                // Lấy JwtService đã đăng ký qua Dependency Injection
                var jwtService = context.HttpContext.RequestServices.GetRequiredService<JwtService>();

                // Xác thực token bằng logic trong JwtService
                var principal = jwtService.ValidateToken(token);

                if (principal != null)
                {
                    // Nếu xác thực thành công, đặt ClaimsPrincipal vào ngữ cảnh.
                    // Việc này là đủ để báo hiệu thành công mà không cần gọi context.Success().
                    context.Principal = principal;
                    return;
                }
            }
            
            // Nếu không gán Principal, ASP.NET Core sẽ tự động xem đây là thất bại 
            // và thực hiện Challenge (chuyển hướng đến LoginPath).
            // Không cần gọi context.RejectPrincipal() vốn gây lỗi.
        };
    })
    // 2. Cấu hình Scheme "Bearer" riêng biệt cho các API Controller (nếu có)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });


var app = builder.Build();
app.MapHub<GameHub>("/gameHub");

// =========================================================
// 2. CẤU HÌNH HTTP REQUEST PIPELINE
// =========================================================

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

// Sử dụng Authentication và Authorization
app.UseAuthentication(); // ⚠️ PHẢI ĐỨNG TRƯỚC UseAuthorization
app.UseAuthorization();

// Các map routes
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
