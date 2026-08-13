using System.Security.Claims;

namespace AutoFlow.API.Services
{
    // Resolve o OficinaId do usuário autenticado, por requisição (scoped).
    public class TenantProvider
    {
        private readonly IHttpContextAccessor? _http;

        public TenantProvider(IHttpContextAccessor? http = null) => _http = http;

        // OficinaId do JWT. Null para SuperAdmin (Role=Admin) ou requisição sem token.
        public Guid? OficinaId
        {
            get
            {
                var claim = _http?.HttpContext?.User?.FindFirst("OficinaId")?.Value;
                return Guid.TryParse(claim, out var id) ? id : null;
            }
        }

        // True quando é SuperAdmin — usado para decidir quando ignorar o filtro.
        public bool IsSuperAdmin =>
            _http?.HttpContext?.User?.IsInRole("Admin") ?? false;
    }
}

