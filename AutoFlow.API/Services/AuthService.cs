using AutoFlow.API.Data;
using AutoFlow.API.DTO.Auth;
using AutoFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoFlow.API.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ========================= REGISTER =========================
        public async Task<bool> Register(RegisterDTO dto)
        {
            var email = dto.Email?.Trim().ToLower();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(dto.Senha))
                return false;

            var exists = await _context.Usuarios
                .AnyAsync(u => u.Email == email);

            if (exists) return false;

            var user = new Usuario
            {
                Email = email,
                Senha = BCrypt.Net.BCrypt.HashPassword(dto.Senha.Trim()),
                Role = "User"
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return true;
        }

        // ========================= LOGIN =========================
        public async Task<string?> Login(LoginDTO dto)
        {
            var email = dto.Email?.Trim().ToLower();
            var senha = dto.Senha?.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                Console.WriteLine("❌ Email ou senha vazios");
                return null;
            }

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Não logar o e-mail completo (dado pessoal — LGPD/CWE-532). Mascarar.
                Console.WriteLine($"❌ Falha de login (usuário não encontrado): {MascararEmail(email)}");
                return null;
            }

            var senhaValida = BCrypt.Net.BCrypt.Verify(senha, user.Senha);

            if (!senhaValida)
            {
                Console.WriteLine($"❌ Falha de login (senha inválida): {MascararEmail(email)}");
                return null;
            }

            Console.WriteLine($"✅ Login bem-sucedido: {MascararEmail(email)}");
            return GenerateToken(user);
        }

        // ========================= MASCARAMENTO DE EMAIL (LGPD) =========================
        // Ex.: "pabllo@gmail.com" -> "p***@g***". Suficiente para correlacionar tentativas
        // sem gravar o dado pessoal completo em log.
        private static string MascararEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return "(email inválido)";

            var partes = email.Split('@');
            var usuario = partes[0];
            var dominio = partes[1];

            string MascararParte(string p) =>
                p.Length <= 1 ? "***" : $"{p[0]}***";

            return $"{MascararParte(usuario)}@{MascararParte(dominio)}";
        }

        // ========================= TOKEN =========================
        private string GenerateToken(Usuario user, Oficina? oficina = null)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                      ?? _config["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
                throw new InvalidOperationException(
                    "JWT_KEY não configurada ou muito curta. Defina a variável de ambiente JWT_KEY.");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            if (oficina != null)
            {
                claims.Add(new Claim("OficinaId", oficina.Id.ToString()));
                claims.Add(new Claim("OficinaSlug", oficina.Slug));
                claims.Add(new Claim("Plano", oficina.Plano?.Nome ?? "trial"));
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ========================= SEED ADMIN =========================
        public async Task<Usuario?> GarantirAdminPadrao()
        {
            var email = "admin@autoflow.com";

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            // 🔒 Se o admin JÁ existe, não fazemos NADA com a senha dele.
            // Nunca resetar a senha de um admin existente a cada boot (era a falha anterior).
            if (user != null)
            {
                Console.WriteLine("ℹ️ SEED: Admin já existe. Nenhuma alteração de senha realizada.");
                return user;
            }

            // Só na PRIMEIRA criação: senha vem de variável de ambiente, nunca hardcoded.
            var senhaInicial = Environment.GetEnvironmentVariable("ADMIN_SEED_PASSWORD");

            if (string.IsNullOrWhiteSpace(senhaInicial))
            {
                // Sem senha configurada = não cria admin com credencial fraca.
                // Configure ADMIN_SEED_PASSWORD no ambiente para o primeiro boot e remova depois.
                Console.WriteLine("⚠️ SEED: ADMIN_SEED_PASSWORD não definida. Admin NÃO foi criado. " +
                                  "Defina a variável no ambiente para provisionar o admin inicial.");
                return null;
            }

            Console.WriteLine("⚠️ SEED: Admin não existe. Criando com senha inicial da variável de ambiente...");

            user = new Usuario
            {
                Email = email,
                Senha = BCrypt.Net.BCrypt.HashPassword(senhaInicial),
                Role = "Admin"
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ SEED: Admin criado -> Email: admin@autoflow.com | Senha: [definida via ambiente]");
            Console.WriteLine("ℹ️ SEED: Recomendado remover ADMIN_SEED_PASSWORD do ambiente e trocar a senha no primeiro login.");

            return user;
        }
    }
}
