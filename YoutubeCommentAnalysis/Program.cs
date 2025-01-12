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

// YouTube API Servisini DI Container'a ekleme
builder.Services.AddScoped<IYoutubeService>(provider =>
{
    var apiKey = builder.Configuration["YoutubeApi:ApiKey"]; // appsettings.json'dan API anahtarını alma
    return new GetYoutubeComments(apiKey); // IYoutubeService olarak döndürülüyor
});

// Servisleri container'e ekleme
builder.Services.AddRazorPages();

// Session desteğini ekleme
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});

// Kimlik doğrulama ve yetkilendirme için gerekli yapılandırmalar
builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.LoginPath = "/Login"; // Yetkisiz erişimlerde yönlendirme yapılacak sayfa
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Çerez süresi
});

var app = builder.Build();

// HTTP request pipeline'ı yapılandırma
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware'ini ekleme
app.UseSession();

// Kimlik doğrulama ve yetkilendirme middleware'ini ekleme
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

