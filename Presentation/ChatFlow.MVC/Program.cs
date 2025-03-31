using ChatFlow.Application.Repositories.Reads;
using ChatFlow.Application.Repositories.Writes;
using ChatFlow.Application.Services;
using ChatFlow.Infrastructure;
using ChatFlow.Infrastructure.Services;
using ChatFlow.MVC.Hubs;
using ChatFlow.Persistence;
using ChatFlow.Persistence.Repositories.AppUser;
using ChatFlow.Persistence.Repositories.AppUserRepo;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    policy.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed((host) => true);
}));

builder.Services.AddSignalR();

// Register HttpContextAccessor
builder.Services.AddHttpContextAccessor();


builder.AddInfrastructureRegister();
builder.Services.AddPersistenceRegister();

// Configure Authentication (Cookie-based)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// Configure Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors();
//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.MapHub<ChatHub>("/chatHub");
app.MapControllers();

app.Run();
