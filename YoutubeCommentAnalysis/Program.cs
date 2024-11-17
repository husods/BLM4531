using Microsoft.EntityFrameworkCore;
using YoutubeCommentAnalysis.UsersData;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL veritabanı bağlantısını ekleyin
builder.Services.AddDbContext<UsersDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
