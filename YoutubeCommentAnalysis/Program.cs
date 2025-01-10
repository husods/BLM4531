using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using YoutubeCommentAnalysis.Services;
using Microsoft.EntityFrameworkCore;
using YoutubeCommentAnalysis.UsersData;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL veritabanı bağlantısını ekleme
builder.Services.AddDbContext<UsersDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// YouTube API Servisini DI Container'a ekliyoruz
builder.Services.AddScoped<IYoutubeService>(provider =>
{
    var apiKey = builder.Configuration["YoutubeApi:ApiKey"]; // appsettings.json'dan API anahtarını al
    return new GetYoutubeComments(apiKey); // IYoutubeService olarak döndürüyoruz
});

// Add services to the container
builder.Services.AddRazorPages();

// Session desteğini ekle
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturumun süresi
    options.Cookie.HttpOnly = true; // Güvenlik için sadece HTTP üzerinden erişilebilir
    options.Cookie.IsEssential = true; // GDPR uyumu için gerekli
});

// Kimlik doğrulama ve yetkilendirme için gerekli yapılandırmaları ekle
builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.LoginPath = "/Login"; // Yetkisiz erişimlerde yönlendirme yapılacak sayfa
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Çerez süresi
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware'ini ekle
app.UseSession();

// Kimlik doğrulama ve yetkilendirme middleware'ini ekle
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

