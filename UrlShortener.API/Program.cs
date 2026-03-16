using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Common.Configurations;
using UrlShortener.Data;
using UrlShortener.Data.Entities;
using UrlShortener.Services;
using UrlShortener.Services.Implementations;
using UrlShortener.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// QUAN TRỌNG: bind AppSettings từ appsettings.json
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

builder.Services.AddScoped<IShortUrlService, ShortUrlService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "UrlShortener API is running");

app.Run();