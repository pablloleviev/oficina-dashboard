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
        policy.WithOrigins(
                "https://autoflow-gestao.vercel.app",
                "http://localhost:5173",
                "http://localhost:3000",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:3000"
            )
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

    if (rawUrl.Contains("://"))
    {
        try
        {
            var uri = new Uri(rawUrl);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : "";
            var host = uri.Host;
            var database = uri.AbsolutePath.TrimStart('/');

            if (rawUrl.StartsWith("postgres", StringComparison.OrdinalIgnoreCase))
            {
                var pgPort = uri.Port != -1 ? $"Port={uri.Port};" : "";
                return $"Host={host};{pgPort}Database={database};Username={user};Password={password};Timeout=15;CommandTimeout=15;SslMode=Require;Trust Server Certificate=true;";
            }

            var portPart = uri.Port != -1 ? $";Port={uri.Port}" : "";
            return $"Server={host}{portPart};Database={database};Uid={user};Pwd={password};Connect Timeout=15";
        }
        catch { return rawUrl; }
    }

    return rawUrl;
}

var baseConnString = GetConnectionString();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
              ?? Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? "";

    if (!string.IsNullOrEmpty(baseConnString) && (baseConnString.Contains("Server=") || baseConnString.Contains("Host=")))
    {
        if (rawUrl.StartsWith("postgres", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("📂 DATABASE: Usando PostgreSQL (Produção/Render)");
            options.UseNpgsql(baseConnString);
        }
        else
        {
            Console.WriteLine("📂 DATABASE: Usando MySQL (Produção/Render)");
            options.UseMySql(baseConnString, new MySqlServerVersion(new Version(8, 0, 32)));
        }
    }
    else
    {
        Console.WriteLine("📂 DATABASE: Usando SQLite (Desenvolvimento)");
        options.UseSqlite("Data Source=autoflow.db");
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
// 🔥 CORS PRIMEIRO — antes de qualquer coisa que possa falhar
app.UseCors("AutoFlowCors");

// 🔥 HANDLER GLOBAL DE EXCEÇÕES — captura qualquer erro 500 e loga a verdade
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine("❌ EXCEÇÃO NÃO TRATADA NA REQUISIÇÃO");
        Console.WriteLine($"   Rota: {context.Request.Method} {context.Request.Path}");
        Console.WriteLine($"   Mensagem: {ex.Message}");
        Console.WriteLine($"   Tipo: {ex.GetType().Name}");
        if (ex.InnerException != null)
            Console.WriteLine($"   Inner: {ex.InnerException.Message}");
        Console.WriteLine($"   Stack:\n{ex.StackTrace}");
        Console.WriteLine("═══════════════════════════════════════════");

        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Erro interno do servidor",
                    debug = ex.Message  // ⚠️ TEMPORÁRIO: vamos remover no final
                })
            );
        }
    }
});

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
        var providerName = context.Database.ProviderName ?? "Unknown";
        Console.WriteLine($"📂 Provider detectado: {providerName}");

        // Se houver migrations pendentes, aplica. Senão, cria o schema do zero.
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

        if (pendingMigrations.Any() || appliedMigrations.Any())
        {
            Console.WriteLine($"📦 Aplicando {pendingMigrations.Count()} migrations pendentes...");
            await context.Database.MigrateAsync();
            Console.WriteLine("✅ MIGRATIONS APLICADAS COM SUCESSO.");
        }
        else
        {
            // Sem migrations no projeto: cria o schema direto do modelo
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine($"✅ SCHEMA CRIADO VIA EnsureCreated ({providerName}).");
        }

        var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
        await authService.GarantirAdminPadrao();
        Console.WriteLine("🚀 SEED DE ADMIN CONCLUÍDO.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ ERRO CRÍTICO NO STARTUP: " + ex.Message);
        if (ex.InnerException != null)
            Console.WriteLine("   -> Detalhes: " + ex.InnerException.Message);
        Console.WriteLine("   -> Stack: " + ex.StackTrace);
        throw;
    }
}

app.Run();