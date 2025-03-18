using ChatFlow.Persistence;
using ChatFlow.Infrastructure;
using Microsoft.OpenApi.Models;
using ChatFlow.Application.Services;
using ChatFlow.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using ChatFlow.Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

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


builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

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
app.MapGet("/api/auth/getuserdatas", [Authorize(Roles = "Admin")] (IAuthService authService) =>
{
    var user = authService.GetUserDatas();

    if (user == null)
        return Results.Problem("Invalid user data", statusCode: StatusCodes.Status401Unauthorized);

    return Results.Ok(user);
});

app.Run();
