using ChatFlow.Persistence;
using ChatFlow.Application;
using ChatFlow.Infrastructure;
using Microsoft.OpenApi.Models;


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

app.Run();
