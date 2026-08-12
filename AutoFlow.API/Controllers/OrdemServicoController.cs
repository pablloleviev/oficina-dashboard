using AutoFlow.API.Exceptions;
using AutoFlow.API.DTO;
using AutoFlow.API.Services;
using AutoFlow.API.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AutoFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdemServicoController : ControllerBase
    {
        private readonly OrdemServicoService _service;

        public OrdemServicoController(OrdemServicoService service)
        {
            _service = service;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst("UserId")?.Value);
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        // ========================= GET =========================
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? status)
        {
            var userId = GetUserId();

            var lista = await _service.GetAll(userId, status);

            return Ok(ApiResponse<object>.SuccessResponse(lista));
        }

        // ========================= CREATE =========================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrdemServicoDTO dto)
        {
            try
            {
                var userId = GetUserId();
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
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrdemServicoDTO dto)
        {
            try
            {
                var userId = GetUserId();

                var resultado = await _service.Update(id, dto, userId);

                if (resultado == null)
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        new List<string> { "Ordem não encontrada" }
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


        // ========================= DELETE =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();

            var sucesso = await _service.Delete(id, userId);

            if (!sucesso)
                return NotFound(ApiResponse<string>.ErrorResponse(
                    new List<string> { "Ordem não encontrada" }
                ));

            return Ok(ApiResponse<string>.SuccessResponse("Deletado com sucesso"));
        }

        // ========================= FATURAR =========================
        [HttpPut("{id}/faturar")]
        public async Task<IActionResult> Faturar(int id)
        {
            try
            {
                var userId = GetUserId();
                var role = GetUserRole();

                var resultado = await _service.Faturar(id, userId, role);

                if (resultado == null)
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        new List<string> { "Ordem não encontrada" }
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

        // ========================= DESFATURAR (AGORA COM SENHA REAL)
        [HttpPut("{id}/desfaturar")]
        public async Task<IActionResult> Desfaturar(int id, [FromBody] DesfaturarDTO dto)
        {
            try
            {
                var userId = GetUserId();
                var role = GetUserRole();

                var resultado = await _service.Desfaturar(id, userId, role, dto.Senha);

                if (resultado == null)
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        new List<string> { "Ordem não encontrada" }
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

        // ========================= LOGS =========================
        [HttpGet("{id}/logs")]
        public async Task<IActionResult> GetLogs(int id)
        {
            var userId = GetUserId();

            var logs = await _service.GetLogsByOrdem(id, userId);

            return Ok(ApiResponse<object>.SuccessResponse(logs));
        }

        // ========================= STATUS =========================
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDTO dto)
        {
            try
            {
                var userId = GetUserId();
                var resultado = await _service.UpdateStatus(id, dto.Status, userId);

                if (resultado == null)
                    return NotFound(ApiResponse<string>.ErrorResponse(
                        new List<string> { "Ordem não encontrada" }
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

    public class StatusUpdateDTO
    {
        public string Status { get; set; }
    }
}