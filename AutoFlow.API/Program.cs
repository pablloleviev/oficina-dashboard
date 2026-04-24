using AutoFlow.API.Data;
using AutoFlow.API.Responses;
using AutoFlow.API.Services;
using AutoFlow.API.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ========================= CONTROLLERS + JSON CAMELCASE =========================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });

// ========================= CORS =========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AutoFlowCors", policy =>
    {
        policy.WithOrigins("https://autoflow-gestao.vercel.app")
            .SetIsOriginAllowed(origin =>
            {
                var uri = new Uri(origin);
                return uri.Host == "localhost" || uri.Host == "127.0.0.1";
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ========================= DATABASE (SUPORTE A URI DO RENDER) =========================
string? GetConnectionString()
{
    var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL") 
              ?? Environment.GetEnvironmentVariable("CONNECTION_STRING");

    if (string.IsNullOrEmpty(rawUrl))
        return builder.Configuration.GetConnectionString("DefaultConnection");

    // Se for uma URI (comum no Render/Heroku), converte para Connection String
    if (rawUrl.Contains("://"))
    {
        try
        {
            var uri = new Uri(rawUrl);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : "";
            var host = uri.Host;
            var port = uri.Port;
            var database = uri.AbsolutePath.TrimStart('/');

            // Retorna formato padrão para SQL Server ou MySQL (ajustado pelo UseMySql/UseSqlServer abaixo)
            return $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};TrustServerCertificate=True;SSL Mode=Required";
        }
        catch { return rawUrl; }
    }

    return rawUrl;
}

var connectionString = GetConnectionString();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Tenta detectar se deve usar MySQL ou SQL Server baseado no provider ou na string
    if (connectionString != null && (connectionString.Contains("Port=3306") || connectionString.Contains("Uid=")))
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// ========================= SERVICES =========================
builder.Services.AddScoped<ServicoService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrdemServicoService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<FinanceiroService>();
builder.Services.AddScoped<RelatoriosService>();

// ========================= VALIDATION =========================
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ServicoValidator>();

// ========================= ERROS PADRÃO =========================
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .SelectMany(x => x.Value.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        return new BadRequestObjectResult(ApiResponse<string>.ErrorResponse(errors));
    };
});

// ========================= JWT =========================
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
          ?? builder.Configuration["Jwt:Key"];

var key = Encoding.UTF8.GetBytes(jwtKey!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// ========================= SWAGGER =========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "AutoFlow.API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ========================= HEALTH CHECK =========================
builder.Services.AddHealthChecks();

var app = builder.Build();

// ========================= SWAGGER =========================
app.UseSwagger();
app.UseSwaggerUI();

// ========================= PIPELINE =========================
app.UseCors("AutoFlowCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ========================= HEALTH CHECK ENDPOINT =========================
app.MapHealthChecks("/health");

// ========================= AUTO-MIGRATE + SEED =========================
Console.WriteLine("🚀 INICIANDO PROCESSO DE MIGRAÇÃO E SEED...");
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Aplica migrations pendentes automaticamente
        await context.Database.MigrateAsync();
        Console.WriteLine("✅ MIGRATIONS APLICADAS COM SUCESSO.");

        var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
        await authService.GarantirAdminPadrao();
        Console.WriteLine("🚀 PROCESSO DE SEED CONCLUÍDO.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ ERRO CRÍTICO NO STARTUP: " + ex.Message);
        if (ex.InnerException != null)
            Console.WriteLine("   -> Detalhes: " + ex.InnerException.Message);
    }
}

app.Run();
