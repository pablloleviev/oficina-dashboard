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
using Npgsql;
using Resend;
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

// ========================= DIAGNÃ“STICO DE VARIÃVEIS DE AMBIENTE =========================
Console.WriteLine("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
Console.WriteLine("ðŸ” DIAGNÃ“STICO DE VARIÃVEIS DE AMBIENTE");

var diagDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var diagConnStr = Environment.GetEnvironmentVariable("CONNECTION_STRING");
var diagJwt = Environment.GetEnvironmentVariable("JWT_KEY");
var diagAspEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

// ðŸ”’ FunÃ§Ã£o local pra mascarar senha (mostra tudo MENOS o que estÃ¡ entre ":" e "@")
static string MascararSenha(string? url)
{
    if (string.IsNullOrEmpty(url)) return "(vazio)";
    
    // Procura padrÃ£o "://USER:SENHA@HOST"
    var schemeEnd = url.IndexOf("://", StringComparison.OrdinalIgnoreCase);
    if (schemeEnd < 0) return url;
    
    var afterScheme = url.Substring(schemeEnd + 3);
    var atIndex = afterScheme.IndexOf('@');
    if (atIndex < 0)
    {
        // SEM @ â€” algo estranho, mostra como estÃ¡ pra debug
        return url + "  [âš ï¸ SEM '@' DETECTADO!]";
    }
    
    var userInfo = afterScheme.Substring(0, atIndex);
    var hostInfo = afterScheme.Substring(atIndex + 1);
    
    // Separa user:senha
    var colonIndex = userInfo.IndexOf(':');
    string userPart;
    if (colonIndex < 0)
    {
        userPart = userInfo + ":(sem senha)";
    }
    else
    {
        var user = userInfo.Substring(0, colonIndex);
        userPart = $"{user}:***SENHA***";
    }
    
    var schemePart = url.Substring(0, schemeEnd + 3);
    return $"{schemePart}{userPart}@{hostInfo}";
}

Console.WriteLine($"   DATABASE_URL presente?     {!string.IsNullOrEmpty(diagDbUrl)}");
Console.WriteLine($"   DATABASE_URL tamanho:      {(diagDbUrl?.Length ?? 0)} caracteres");
Console.WriteLine($"   DATABASE_URL (mascarada):  {MascararSenha(diagDbUrl)}");
Console.WriteLine($"   CONNECTION_STRING presente? {!string.IsNullOrEmpty(diagConnStr)}");
Console.WriteLine($"   JWT_KEY presente?          {!string.IsNullOrEmpty(diagJwt)}");
Console.WriteLine($"   ASPNETCORE_ENVIRONMENT:    {diagAspEnv ?? "(nÃ£o definido)"}");
Console.WriteLine("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");

// ========================= DATABASE â€” PARSER MANUAL SIMPLES =========================
// Parser direto, sem System.Uri (que rejeita URLs sem porta explÃ­cita).
// Formato esperado: postgresql://user:pass@host[:port]/database
string? GetConnectionString()
{
    var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
              ?? Environment.GetEnvironmentVariable("CONNECTION_STRING");

    if (string.IsNullOrEmpty(rawUrl))
    {
        Console.WriteLine("â„¹ï¸  GetConnectionString: nenhuma env var, usando appsettings.json");
        return builder.Configuration.GetConnectionString("DefaultConnection");
    }

    // Se NÃƒO contÃ©m "://", jÃ¡ Ã© uma connection string completa
    if (!rawUrl.Contains("://"))
    {
        Console.WriteLine("â„¹ï¸  GetConnectionString: URL jÃ¡ Ã© connection string, retornando direto");
        return rawUrl;
    }

    bool isPostgres = rawUrl.StartsWith("postgres", StringComparison.OrdinalIgnoreCase);

    try
    {
        // ===== Parsing manual passo a passo =====
        // Exemplo: postgresql://user:pass@host:5432/dbname

        // 1) Remove o scheme (postgresql:// ou postgres:// ou mysql://)
        var schemeEnd = rawUrl.IndexOf("://", StringComparison.OrdinalIgnoreCase);
        if (schemeEnd < 0) throw new FormatException("Sem '://' na URL");
        var withoutScheme = rawUrl.Substring(schemeEnd + 3);

        // 2) Separa "userInfo@hostInfo/dbAndQuery"
        var atIndex = withoutScheme.IndexOf('@');
        if (atIndex < 0) throw new FormatException("Sem '@' separando user:pass de host");

        var userInfo = withoutScheme.Substring(0, atIndex);
        var hostAndDb = withoutScheme.Substring(atIndex + 1);

        // 3) Separa user:pass
        var userColonIndex = userInfo.IndexOf(':');
        string user, pass;
        if (userColonIndex < 0)
        {
            user = userInfo;
            pass = "";
        }
        else
        {
            user = userInfo.Substring(0, userColonIndex);
            pass = userInfo.Substring(userColonIndex + 1);
        }
        user = Uri.UnescapeDataString(user);
        pass = Uri.UnescapeDataString(pass);

        // 4) Separa "host[:port]/database[?query]"
        var slashIndex = hostAndDb.IndexOf('/');
        if (slashIndex < 0) throw new FormatException("Sem '/' separando host de database");

        var hostPart = hostAndDb.Substring(0, slashIndex);
        var dbPart = hostAndDb.Substring(slashIndex + 1);

        // 5) Remove eventual ?query=string do nome do database
        var queryIndex = dbPart.IndexOf('?');
        if (queryIndex >= 0) dbPart = dbPart.Substring(0, queryIndex);
        var db = dbPart;

        // 6) Separa host:port (porta Ã© opcional)
        string host;
        int port = -1;
        var hostColonIndex = hostPart.LastIndexOf(':');
        if (hostColonIndex < 0)
        {
            host = hostPart;
        }
        else
        {
            host = hostPart.Substring(0, hostColonIndex);
            var portStr = hostPart.Substring(hostColonIndex + 1);
            if (!int.TryParse(portStr, out port))
            {
                Console.WriteLine($"âš ï¸  Porta invÃ¡lida ('{portStr}'), usando default");
                port = -1;
            }
        }

        Console.WriteLine($"âœ… GetConnectionString: parse OK â€” host={host}, db={db}, user={user}, port={(port == -1 ? "(default 5432)" : port.ToString())}");

        if (isPostgres)
        {
            // Usa NpgsqlConnectionStringBuilder â€” robusto e nativo
            var pgBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Database = db,
                Username = user,
                Password = pass,
                SslMode = SslMode.Require,
                TrustServerCertificate = true,
                Timeout = 15,
                CommandTimeout = 15
            };
            if (port != -1) pgBuilder.Port = port;
            // Se port == -1, Npgsql usa 5432 por padrÃ£o automaticamente

            return pgBuilder.ToString();
        }
        else
        {
            // MySQL
            var portPart = port != -1 ? $";Port={port}" : "";
            return $"Server={host}{portPart};Database={db};Uid={user};Pwd={pass};Connect Timeout=15";
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"âš ï¸  GetConnectionString: erro ao parsear ({ex.GetType().Name}): {ex.Message}");
        Console.WriteLine($"âš ï¸  Retornando URL crua. App provavelmente cairÃ¡ no SQLite.");
        return rawUrl;
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
            Console.WriteLine("ðŸ“‚ DATABASE: Usando PostgreSQL (ProduÃ§Ã£o/Render)");
            options.UseNpgsql(baseConnString);
        }
        else
        {
            Console.WriteLine("ðŸ“‚ DATABASE: Usando MySQL (ProduÃ§Ã£o/Render)");
            options.UseMySql(baseConnString, new MySqlServerVersion(new Version(8, 0, 32)));
        }
    }
    else
    {
        Console.WriteLine("ðŸ“‚ DATABASE: Usando SQLite (Desenvolvimento)");
        options.UseSqlite("Data Source=autoflow.db");
    }

    // ðŸ”¥ SUPRIME o warning chato do EF Core 9 sobre PendingModelChanges
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
builder.Services.AddHttpContextAccessor();
// ========================= RESEND (EMAIL) =========================
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? "";
});
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<TenantService>();

// ========================= VALIDATION =========================
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ServicoValidator>();

// ========================= ERROS PADRÃƒO =========================
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

// ðŸ”¥ HANDLER GLOBAL DE EXCEÃ‡Ã•ES
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
        Console.WriteLine("âŒ EXCEÃ‡ÃƒO NÃƒO TRATADA NA REQUISIÃ‡ÃƒO");
        Console.WriteLine($"   Rota: {context.Request.Method} {context.Request.Path}");
        Console.WriteLine($"   Mensagem: {ex.Message}");
        Console.WriteLine($"   Tipo: {ex.GetType().Name}");
        if (ex.InnerException != null)
            Console.WriteLine($"   Inner: {ex.InnerException.Message}");
        Console.WriteLine($"   Stack:\n{ex.StackTrace}");
        Console.WriteLine("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");

        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Erro interno do servidor"
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
Console.WriteLine("ðŸš€ INICIANDO PROCESSO DE MIGRAÃ‡ÃƒO E SEED...");
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var providerName = context.Database.ProviderName ?? "Unknown";
        Console.WriteLine($"ðŸ“‚ Provider detectado: {providerName}");

        Console.WriteLine("ðŸ“¦ Criando schema via EnsureCreatedAsync...");
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine($"âœ… SCHEMA CRIADO/VERIFICADO ({providerName}).");

        var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
        await authService.GarantirAdminPadrao();
        Console.WriteLine("ðŸš€ SEED DE ADMIN CONCLUÃDO.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("âŒ ERRO CRÃTICO NO STARTUP: " + ex.Message);
        if (ex.InnerException != null)
            Console.WriteLine("   -> Detalhes: " + ex.InnerException.Message);
        Console.WriteLine("   -> Stack: " + ex.StackTrace);
        throw;
    }
}

app.Run();