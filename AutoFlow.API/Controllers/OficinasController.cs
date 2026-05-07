using AutoFlow.API.Data;
using AutoFlow.API.Models;
using AutoFlow.API.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OficinasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OficinasController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/oficinas — lista todas (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var oficinas = await _context.Oficinas
                .Include(o => o.Plano)
                .OrderByDescending(o => o.CriadoEm)
                .ToListAsync();

            return Ok(ApiResponse<object>.SuccessResponse(oficinas));
        }

        // POST /api/oficinas — cadastra nova oficina
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CriarOficinaDTO dto)
        {
            if (await _context.Oficinas.AnyAsync(o => o.Slug == dto.Slug))
                return BadRequest(ApiResponse<string>.ErrorResponse(new List<string> { "Slug já em uso." }));

            if (await _context.Oficinas.AnyAsync(o => o.Email == dto.Email))
                return BadRequest(ApiResponse<string>.ErrorResponse(new List<string> { "Email já cadastrado." }));

            var plano = await _context.Planos.FindAsync(dto.PlanoId);
            if (plano == null)
                return BadRequest(ApiResponse<string>.ErrorResponse(new List<string> { "Plano inválido." }));

            var oficina = new Oficina
            {
                Nome = dto.Nome,
                Slug = dto.Slug.ToLower().Trim(),
                Cnpj = dto.Cnpj,
                Email = dto.Email,
                Telefone = dto.Telefone,
                PlanoId = dto.PlanoId,
                Status = "trial",
                TrialAte = DateTime.UtcNow.AddDays(14),
                SchemaName = $"oficina_{dto.Slug.ToLower().Trim()}"
            };

            _context.Oficinas.Add(oficina);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { oficina.Id, oficina.Slug, oficina.TrialAte }));
        }

        // GET /api/oficinas/planos — lista planos disponíveis (público)
        [HttpGet("planos")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlanos()
        {
            var planos = await _context.Planos
                .Where(p => p.Ativo)
                .OrderBy(p => p.Preco)
                .ToListAsync();

            return Ok(ApiResponse<object>.SuccessResponse(planos));
        }
    }

    public class CriarOficinaDTO
    {
        public string Nome { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? Cnpj { get; set; }
        public string Email { get; set; } = "";
        public string? Telefone { get; set; }
        public Guid PlanoId { get; set; }
    }
}
