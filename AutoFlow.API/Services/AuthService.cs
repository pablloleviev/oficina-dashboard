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
                Console.WriteLine("❌ Usuário não encontrado");
                return null;
            }

            // Verificação de senha (sem logs de debug em produção)
            var senhaValida = BCrypt.Net.BCrypt.Verify(senha, user.Senha);

            if (!senhaValida)
            {
                Console.WriteLine($"❌ Falha de login para: {email}");
                return null;
            }

            Console.WriteLine($"✅ Login bem-sucedido: {email}");
            return GenerateToken(user);
        }

        // ========================= TOKEN =========================
        private string GenerateToken(Usuario user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ========================= RESET DEFINITIVO =========================
        public async Task ResetAcessoDefinitivo()
        {
            Console.WriteLine("🧹 INICIANDO LIMPEZA DE USUÁRIOS...");
            
            // 1. Limpeza total
            var todosUsuarios = await _context.Usuarios.ToListAsync();
            _context.Usuarios.RemoveRange(todosUsuarios);
            await _context.SaveChangesAsync();
            
            Console.WriteLine("✅ TABELA DE USUÁRIOS LIMPA.");

            // 2. Criação do Admin
            var admin = new Usuario
            {
                Email = "admin@autoflow.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("AutoFlow@123"),
                Role = "Admin"
            };

            _context.Usuarios.Add(admin);
            await _context.SaveChangesAsync();

            Console.WriteLine("USUÁRIO ADMIN CRIADO COM HASH VALIDADO: admin@autoflow.com");
        }

        public async Task<Usuario> GarantirAdminPadrao()
        {
            var email = "admin@autoflow.com";

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                Console.WriteLine("⚠️ SEED: Admin não existe. Criando novo...");

                user = new Usuario
                {
                    Email = email,
                    Senha = BCrypt.Net.BCrypt.HashPassword("AutoFlow@123"),
                    Role = "Admin"
                };

                _context.Usuarios.Add(user);
            }
            else
            {
                Console.WriteLine("⚠️ SEED: Admin encontrado. Forçando reset de senha e role...");

                user.Senha = BCrypt.Net.BCrypt.HashPassword("AutoFlow@123");
                user.Role = "Admin";
                
                // 🔥 GARANTE QUE O EF CORE VEJA A MUDANÇA
                _context.Entry(user).State = EntityState.Modified;
            }

            var rows = await _context.SaveChangesAsync();
            
            if (rows > 0)
                Console.WriteLine($"✅ SEED: Banco de dados atualizado com sucesso ({rows} colunas afetadas).");
            else
                Console.WriteLine("ℹ️ SEED: Nenhuma alteração pendente (Usuário já estava correto).");

            Console.WriteLine("✅ SEED: Admin garantido -> Email: admin@autoflow.com | Senha: AutoFlow@123");

            return user;
        }
    }
}
