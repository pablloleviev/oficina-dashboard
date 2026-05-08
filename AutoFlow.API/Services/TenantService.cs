using AutoFlow.API.Data;
using AutoFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow.API.Services
{
    public class TenantService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public string? ObterSlug()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Lê o header X-Tenant enviado pelo frontend
            if (context.Request.Headers.TryGetValue("X-Tenant", out var tenant))
                return tenant.ToString().ToLower().Trim();

            return null;
        }

        public async Task<Oficina?> ObterOficinaAsync()
        {
            var slug = ObterSlug();
            if (string.IsNullOrEmpty(slug)) return null;

            return await _context.Oficinas
                .Include(o => o.Plano)
                .FirstOrDefaultAsync(o => o.Slug == slug && o.Status == "ativo");
        }

        public async Task<bool> OficinaAtivaAsync()
        {
            var oficina = await ObterOficinaAsync();
            return oficina != null;
        }
    }
}
