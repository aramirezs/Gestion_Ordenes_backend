using Api_Ordenes.Middlewares;
using Application.Commands;
using Application.Interfaces;
using FluentValidation.AspNetCore;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using System.Reflection;
using Application.Behaviors;
using Application.Validators;

var builder = WebApplication.CreateBuilder(args);

var serviceName = builder.Configuration.GetValue<string>("AppSettings:ServiceName");
var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gestion de Ordenes API", Version = "v1" });

    // 🔒 Definición del esquema de seguridad
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header usando el esquema Bearer.  
                        Escribe 'Bearer' seguido de un espacio y tu token.  
                        Ejemplo: Bearer abcdef12345",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // 🔒 Requerimiento global de seguridad
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Configuración MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CrearOrdenCommand).Assembly));

builder.Services.AddScoped<IUnitOfWork>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new UnitOfWork(connectionString);
});

// Configuración UnitOfWork (Scoped)
//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,    
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

            RoleClaimType = ClaimTypes.Role
        };
    });

    builder.Services.AddControllers()
        .AddFluentValidation(fv =>
        {
            // Registra todos los validadores en el ensamblado
            fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        });

// FluentValidation y MediatR
builder.Services.AddValidatorsFromAssemblyContaining<CrearOrdenCommandValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));




builder.Services.AddAuthorization();


#region CORS

builder.Services.AddCors(o =>
{
    o.AddPolicy("CorsPolicy",
          builder => builder
              .WithOrigins(new string[] {
                "http://localhost:3000" ,
                "http://localhost:3001" ,
                "http://localhost:5173" ,
                "http://localhost:5174" ,
                "https://lobytech.com",
                "https://www.lobytech.com",
                "https://main.d29g2b1txb4hus.amplifyapp.com"
              })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

#endregion


var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseCustomMiddleware();

app.UseAuthorization();

app.MapControllers();

app.Run();
