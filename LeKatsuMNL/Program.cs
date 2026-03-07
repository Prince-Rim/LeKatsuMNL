using LeKatsuMNL.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Authentication.Cookies;

// Load the .env file containing API Keys (like EmailJS)
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// Add Session Services for OTP verification
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); // OTP expires in 10 minutes
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register DbContext
builder.Services.AddDbContext<LeKatsuMNL.Data.LeKatsuDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISsaForecastingService, SsaForecastingService>();
builder.Services.AddScoped<ILstmForecastingService, LstmForecastingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRewriter(new Microsoft.AspNetCore.Rewrite.RewriteOptions()
    .AddRedirect("^$", "Login/login"));

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

