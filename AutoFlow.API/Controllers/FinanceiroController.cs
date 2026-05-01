using AutoFlow.API.DTO.Financeiro;
using AutoFlow.API.Responses;
using AutoFlow.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FinanceiroController : ControllerBase
    {
        private readonly FinanceiroService _financeiroService;

        public FinanceiroController(FinanceiroService financeiroService)
        {
            _financeiroService = financeiroService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        }

        // ========================= GET RESUMO =========================
        [HttpGet("resumo")]
        public async Task<IActionResult> GetResumo()
        {
            var userId = GetUserId();
            var resumo = await _financeiroService.GetResumo(userId);
            return Ok(ApiResponse<ResumoFinanceiroDTO>.SuccessResponse(resumo));
        }

        // ========================= GET TRANSAÇÕES =========================
        [HttpGet("transacoes")]
        public async Task<IActionResult> GetTransacoes()
        {
            var userId = GetUserId();
            var transacoes = await _financeiroService.GetTransacoes(userId);
            return Ok(ApiResponse<List<TransacaoDTO>>.SuccessResponse(transacoes));
        }

        // ========================= GET DESPESAS =========================
        [HttpGet("despesas")]
        public async Task<IActionResult> GetDespesas()
        {
            var userId = GetUserId();
            var despesas = await _financeiroService.GetDespesas(userId);
            return Ok(ApiResponse<List<DespesaDTO>>.SuccessResponse(despesas));
        }

        // ========================= CREATE DESPESA =========================
        [HttpPost("despesas")]
        public async Task<IActionResult> CreateDespesa([FromBody] DespesaDTO dto)
        {
            var userId = GetUserId();
            var result = await _financeiroService.CreateDespesa(dto, userId);
            return Ok(ApiResponse<DespesaDTO>.SuccessResponse(result));
        }

        // ========================= UPDATE DESPESA =========================
        [HttpPut("despesas/{id}")]
        public async Task<IActionResult> UpdateDespesa(int id, [FromBody] DespesaDTO dto)
        {
            var userId = GetUserId();
            var result = await _financeiroService.UpdateDespesa(id, dto, userId);

            if (result == null)
                return NotFound(ApiResponse<string>.ErrorResponse(new List<string> { "Despesa não encontrada" }));

            return Ok(ApiResponse<DespesaDTO>.SuccessResponse(result));
        }

        // ========================= DELETE DESPESA =========================
        [HttpDelete("despesas/{id}")]
        public async Task<IActionResult> DeleteDespesa(int id)
        {
            var userId = GetUserId();
            var sucesso = await _financeiroService.DeleteDespesa(id, userId);

            if (!sucesso)
                return NotFound(ApiResponse<string>.ErrorResponse(new List<string> { "Despesa não encontrada" }));

            return Ok(ApiResponse<string>.SuccessResponse("Despesa excluída com sucesso"));
        }
    }
}
