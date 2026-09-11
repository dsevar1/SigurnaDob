using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SigurnaDob.Api.Data;
using SigurnaDob.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SigurnaDob.Api", Version = "v1" });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Unesi JWT token dobiven od /api/auth/login (bez 'Bearer ' prefiksa)."
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document, null), new List<string>() }
    });
});

builder.Services.AddDbContext<SigurnaDobDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// NAMJERNO bez UseHttpsRedirection(): launchSettings.json "https" profil veže Kestrel na http i https
// istovremeno, pa bi ovaj middleware 307-redirectao svaki http poziv na https. HttpClient (App strane)
// automatski prati redirect, ali .NET tada NAMJERNO briše Authorization header jer redirect ide na
// drugi port/shemu (cross-origin sigurnosno pravilo) - token nikad nije stigao do Api-ja, 401 na sve
// zaštićene endpointe. App i Api komuniciraju isključivo preko http://localhost na istom stroju, pa
// prisilan https redirect ovdje nema sigurnosnu svrhu, samo je tiho lomio autorizaciju.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
