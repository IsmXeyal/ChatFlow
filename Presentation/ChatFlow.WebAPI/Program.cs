using ChatFlow.Persistence;
using ChatFlow.Infrastructure;
using Microsoft.OpenApi.Models;
using ChatFlow.Application.Services;
using ChatFlow.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using ChatFlow.Infrastructure.Services;
using ChatFlow.Domain.ViewModels;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHttpClient();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.AddInfrastructureRegister();
builder.Services.AddPersistenceRegister();
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();

// Swagger Authentication Options
// Options elave etdikden sonra Swagger-in sag hissesinde Authorize deye bir hisse gelir
// O hissenin meqsedi Login bize bir key qaytaracaq, Login olandan sonra hansisa token-lere catmaq
// isteyiriksek onu Authorize-da vereceyik.
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // Gondereceyimiz Token requestin header-inde getmelidir.
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});


// Add Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllers();

// Register endpoint
app.MapPost("/api/auth/register", async (IAuthService authService, AppUserDTO appUserDTO) =>
{
    if (appUserDTO == null)
        return Results.BadRequest("User data is required.");

    var result = await authService.AddUserAsync(appUserDTO);
    return Results.Ok(result);
});

// Login endpoint
app.MapPost("/api/auth/login", async (IAuthService authService, LoginDTO loginDTO) =>
{
    var result = await authService.LoginAsync(loginDTO);
    return Results.Ok(result);
});

// Email confirmation endpoint
app.MapGet("/api/auth/emailconfirm", async (IAuthService authService, string token) =>
{
    var result = await authService.EmailConfirmAsync(token);
    return Results.Ok(result);
});

// Forgetpassword endpoint
app.MapPost("/api/auth/forgetpassword", async (IAuthService authService, ForgetPasswordDTO forgetPasswordDTO) =>
{
    var result = await authService.ForgetPasswordAsync(forgetPasswordDTO);
    return Results.Ok(result);
});

// Resetpassword endpoint
app.MapPost("/api/auth/resetpassword", async (IAuthService authService, string token, ResetPasswordDTO resetPasswordDTO) =>
{
    var result = await authService.ResetPasswordAsync(token, resetPasswordDTO);
    return Results.Ok(result);
});

// Refreshtoken endpoint
app.MapPost("/api/auth/refreshtoken", async (IAuthService authService, string refreshToken) =>
{
    var result = await authService.RefreshTokenAsync(refreshToken);
    return Results.Ok(result);
});

// Getuserdata endpoint (only accessible by Admin role)
app.MapGet("/api/auth/getuserdatas", [Authorize(Roles = "Admin")] (IAppUserService appUserService) =>
{
    var user = appUserService.GetUserDatas();

    if (user == null)
        return Results.Problem("Invalid user data", statusCode: StatusCodes.Status401Unauthorized);

    var userVm = new AppUserVM
    {
        UserName = user.UserName,
        ConnectionId = user.ConnectionId
    };

    return Results.Ok(userVm);
});

// Edit user data endpoint
app.MapPut("/api/auth/edituserdata", async (IAppUserService appUserService, int userId, EditUserDTO editUserDTO) =>
{
    var result = await appUserService.EditUserAsync(userId, editUserDTO);
    if (!result)
        return Results.Problem("Failed to update user data", statusCode: StatusCodes.Status400BadRequest);

    return Results.Ok("User data updated successfully.");
});

app.Run();
