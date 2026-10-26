using cift_anadal_yandal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("CiftAnadalYandalDbContext") ?? throw new InvalidOperationException("Connection string 'CiftAnadalYandalDbContext' not found.");

builder.Services.AddDbContext<CiftAnadalYandalDbContext>(options => options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllersWithViews();
// Kimlik Doğrulama (Authentication) Servislerini Ekliyoruz
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Kullanicilar/Login"; // Giriş yapmayanlar bu sayfaya yönlendirilecek
        options.AccessDeniedPath = "/Kullanicilar/AccessDenied"; // Yetkisi olmayanlar bu sayfaya yönlendirilecek
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Oturum 60 dakika açık kalacak
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication(); // Bu kod UseAuthorization'dan ÖNCE gelmek zorundadır
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
