using AutoFlow.API.DTO.Relatorios;
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
    public class RelatoriosController : ControllerBase
    {
        private readonly RelatoriosService _relatoriosService;

        public RelatoriosController(RelatoriosService relatoriosService)
        {
            _relatoriosService = relatoriosService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        }

        [HttpGet("evolucao-faturamento")]
        public async Task<IActionResult> GetEvolucao()
        {
            var userId = GetUserId();
            var relatorio = await _relatoriosService.GetEvolucaoFaturamento(userId);
            return Ok(ApiResponse<List<EvolucaoFaturamentoDTO>>.SuccessResponse(relatorio));
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = GetUserId();
            var stats = await _relatoriosService.GetDashboardStats(userId);
            return Ok(ApiResponse<DashboardStatsDTO>.SuccessResponse(stats));
        }

        [HttpGet("top-clientes")]
        public async Task<IActionResult> GetTopClientes([FromQuery] int limite = 5)
        {
            var userId = GetUserId();
            var relatorio = await _relatoriosService.GetTopClientes(userId, limite);
            return Ok(ApiResponse<List<TopClienteDTO>>.SuccessResponse(relatorio));
        }

        [HttpGet("top-servicos")]
        public async Task<IActionResult> GetTopServicos([FromQuery] int limite = 5)
        {
            var userId = GetUserId();
            var relatorio = await _relatoriosService.GetTopServicos(userId, limite);
            return Ok(ApiResponse<List<TopServicoDTO>>.SuccessResponse(relatorio));
        }

        [HttpGet("atividade")]
        public async Task<IActionResult> GetAtividade([FromQuery] int dias = 30)
        {
            var userId = GetUserId();
            var relatorio = await _relatoriosService.GetAtividadeHeatmap(userId, dias);
            return Ok(ApiResponse<List<AtividadeHeatmapDTO>>.SuccessResponse(relatorio));
        }
    }
}
