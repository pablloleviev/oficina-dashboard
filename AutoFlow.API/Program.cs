using AutoFlow.API.Data;
using AutoFlow.API.Responses;
using AutoFlow.API.Services;
using AutoFlow.API.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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

// ========================= DIAGNÓSTICO DE VARIÁVEIS DE AMBIENTE =========================
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("🔍 DIAGNÓSTICO DE VARIÁVEIS DE AMBIENTE");

var diagDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var diagConnStr = Environment.GetEnvironmentVariable("CONNECTION_STRING");
var diagJwt = Environment.GetEnvironmentVariable("JWT_KEY");
var diagAspEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

Console.WriteLine($"   DATABASE_URL presente?     {!string.IsNullOrEmpty(diagDbUrl)}");
Console.WriteLine($"   DATABASE_URL tamanho:      {(diagDbUrl?.Length ?? 0)} caracteres");
Console.WriteLine($"   DATABASE_URL começa com:   {(string.IsNullOrEmpty(diagDbUrl) ? "(vazio)" : diagDbUrl.Substring(0, Math.Min(20, diagDbUrl.Length)) + "...")}");
Console.WriteLine($"   CONNECTION_STRING presente? {!string.IsNullOrEmpty(diagConnStr)}");
Console.WriteLine($"   JWT_KEY presente?          {!string.IsNullOrEmpty(diagJwt)}");
Console.WriteLine($"   ASPNETCORE_ENVIRONMENT:    {diagAspEnv ?? "(não definido)"}");
Console.WriteLine("═══════════════════════════════════════════");

// ========================= DATABASE (PARSING ROBUSTO DA URI) =========================
// 🔥 Função que converte URLs do tipo "postgresql://user:pass@host:port/db"
// em connection strings que o Npgsql/MySql entendem.
// Usa parsing manual em vez de System.Uri (que falha com schemes não-HTTP).
string? GetConnectionString()
{
    var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
              ?? Environment.GetEnvironmentVariable("CONNECTION_STRING");

    if (string.IsNullOrEmpty(rawUrl))
    {
        Console.WriteLine("⚠️  GetConnectionString: nenhuma env var, usando appsettings.json");
        return builder.Configuration.GetConnectionString("DefaultConnection");
    }

    // Se NÃO contém "://", já é uma connection string completa, retorna direto
    if (!rawUrl.Contains("://"))
    {
        Console.WriteLine("ℹ️  GetConnectionString: URL já é connection string, retornando direto");
        return rawUrl;
    }

    // Detecta se é Postgres ou MySQL pelo prefixo
    bool isPostgres = rawUrl.StartsWith("postgres", StringComparison.OrdinalIgnoreCase);

    // Regex para extrair os componentes da URI: scheme://user:pass@host:port/database
    var match = Regex.Match(
        rawUrl,
        @"^(?<scheme>\w+)://(?<user>[^:]+):(?<pass>[^@]+)@(?<host>[^:/]+)(?::(?<port>\d+))?/(?<db>[^?]+)",
        RegexOptions.IgnoreCase
    );

    if (!match.Success)
    {
        Console.WriteLine($"⚠️  GetConnectionString: NÃO consegui parsear a URL. Retornando crua.");
        return rawUrl;
    }

    var user = match.Groups["user"].Value;
    var pass = match.Groups["pass"].Value;
    var host = match.Groups["host"].Value;
    var port = match.Groups["port"].Success ? match.Groups["port"].Value : "";
    var db = match.Groups["db"].Value;

    Console.WriteLine($"✅ GetConnectionString: parse OK — host={host}, db={db}, user={user}, port={(string.IsNullOrEmpty(port) ? "(default)" : port)}");

    if (isPostgres)
    {
        var portPart = !string.IsNullOrEmpty(port) ? $"Port={port};" : "";
        return $"Host={host};{portPart}Database={db};Username={user};Password={pass};Timeout=15;CommandTimeout=15;SslMode=Require;Trust Server Certificate=true;";
    }
    else
    {
        // MySQL
        var portPart = !string.IsNullOrEmpty(port) ? $";Port={port}" : "";
        return $"Server={host}{portPart};Database={db};Uid={user};Pwd={pass};Connect Timeout=15";
    }
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

    // 🔥 SUPRIME o warning chato do EF Core 9 sobre PendingModelChanges
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
    );
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
                    debug = ex.Message
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

        // 🔥 SEM MIGRATIONS NO PROJETO: cria o schema direto do modelo
        // (isso funciona em qualquer banco — Postgres, MySQL, SQLite)
        Console.WriteLine("📦 Criando schema via EnsureCreatedAsync...");
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine($"✅ SCHEMA CRIADO/VERIFICADO ({providerName}).");

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