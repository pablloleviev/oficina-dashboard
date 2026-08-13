using AutoFlow.API.Exceptions;
using AutoFlow.API.DTO.Clientes;
using AutoFlow.API.DTO.Relatorios;
using AutoFlow.API.Services;
using AutoFlow.API.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly ClienteService _service;

        public ClientesController(ClienteService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst("UserId")?.Value);
        }

        // ========================= GET ALL =========================
        /// <summary>
        /// GET /api/clientes
        /// Retorna a lista de clientes ativos do usuário autenticado.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await _service.GetAll();
            return Ok(ApiResponse<object>.SuccessResponse(lista));
        }

        // ========================= GET STATS =========================
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var userId = GetUserId();
            // Redireciona para o serviço de BI para consistência
            var relService = HttpContext.RequestServices.GetService<RelatoriosService>();
            if (relService == null) return StatusCode(500);
            
            var stats = await relService.GetDashboardStats(userId);
            return Ok(ApiResponse<DashboardStatsDTO>.SuccessResponse(stats));
        }

        // ========================= GET BY ID =========================
        /// <summary>
        /// GET /api/clientes/{id}
        /// Retorna o detalhe de um cliente ativo. 404 se inativo ou não encontrado.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            var cliente = await _service.GetById(id, userId);

            if (cliente == null)
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Cliente não encontrado" }
                ));

            return Ok(ApiResponse<object>.SuccessResponse(cliente));
        }

        // ========================= CREATE =========================
        /// <summary>
        /// POST /api/clientes
        /// Cria um novo cliente. Validação via FluentValidation (ClienteValidator).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClienteInputDTO dto)
        {
            var userId = GetUserId();

            try
            {
                var resultado = await _service.Create(dto, userId);
                return Ok(ApiResponse<object>.SuccessResponse(resultado));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    new List<string> { ex.Message }
                ));
            }
        }

        // ========================= UPDATE =========================
        /// <summary>
        /// PUT /api/clientes/{id}
        /// Atualiza um cliente ativo. 404 se inativo ou não encontrado.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ClienteInputDTO dto)
        {
            var userId = GetUserId();

            try
            {
                var resultado = await _service.Update(id, dto, userId);

                if (resultado == null)
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        new List<string> { "Cliente não encontrado" }
                    ));

                return Ok(ApiResponse<object>.SuccessResponse(resultado));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    new List<string> { ex.Message }
                ));
            }
        }

        // ========================= DELETE (SOFT) =========================
        /// <summary>
        /// DELETE /api/clientes/{id}
        /// Soft delete: marca o cliente como inativo (IsActive = false).
        /// O registro NÃO é removido do banco.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();

            var sucesso = await _service.Delete(id, userId);

            if (!sucesso)
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Cliente não encontrado" }
                ));

            return Ok(ApiResponse<string>.SuccessResponse("Cliente desativado com sucesso"));
        }
        // ========================= ADD VEÍCULO =========================
        /// <summary>
        /// POST /api/clientes/{clienteId}/veiculos
        /// Adiciona um novo veículo ao cliente, sem exigir dados completos do cliente.
        /// </summary>
        [HttpPost("{clienteId}/veiculos")]
        public async Task<IActionResult> AddVeiculo(int clienteId, [FromBody] AutoFlow.API.DTO.Clientes.VeiculoDTO dto)
        {
            var userId = GetUserId();

            try
            {
                var resultado = await _service.AddVeiculo(clienteId, dto, userId);

                if (resultado == null)
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        new List<string> { "Cliente não encontrado" }
                    ));

                return Ok(ApiResponse<object>.SuccessResponse(resultado));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    new List<string> { ex.Message }
                ));
            }
        }
    }
}
