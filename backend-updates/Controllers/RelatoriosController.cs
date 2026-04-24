using AutoFlow.API.DTO;
using AutoFlow.API.Responses;
using AutoFlow.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // GET /api/relatorios/resumo
        // Response:
        // {
        //   crescimentoMensal, faturamento, ordensNoMes, clientesAtivos,
        //   taxaConclusao, tempoMedioOsDias, retornoClientes
        // }
        [HttpGet("resumo")]
        public async Task<IActionResult> GetResumo()
        {
            var resumo = await _relatoriosService.GetResumoAsync();
            return Ok(new ApiResponse<RelatorioResumoDTO>(true, "Resumo do relatório", resumo));
        }

        // GET /api/relatorios/evolucao-faturamento
        // Response: [ { mes:"jan", valorAtual:2456, valorAnterior:1980 }, … ]  (6 pontos)
        [HttpGet("evolucao-faturamento")]
        public async Task<IActionResult> GetEvolucaoFaturamento()
        {
            var dados = await _relatoriosService.GetEvolucaoFaturamentoAsync();
            return Ok(new ApiResponse<List<EvolucaoFaturamentoDTO>>(true, "Evolução do faturamento", dados));
        }

        // GET /api/relatorios/top-clientes?limit=4
        // Response: [ { posicao:1, nome:"Roberto Ferreira", totalOrdens:5, valorTotal:1200 }, … ]
        [HttpGet("top-clientes")]
        public async Task<IActionResult> GetTopClientes([FromQuery] int limit = 4)
        {
            var dados = await _relatoriosService.GetTopClientesAsync(limit);
            return Ok(new ApiResponse<List<TopClienteDTO>>(true, "Top clientes", dados));
        }

        // GET /api/relatorios/top-servicos?limit=4
        // Response: [ { posicao:1, nome:"Troca de Óleo", frequencia:18, valorTotal:750 }, … ]
        [HttpGet("top-servicos")]
        public async Task<IActionResult> GetTopServicos([FromQuery] int limit = 4)
        {
            var dados = await _relatoriosService.GetTopServicosAsync(limit);
            return Ok(new ApiResponse<List<TopServicoDTO>>(true, "Top serviços", dados));
        }

        // GET /api/relatorios/atividade
        // Response: [ { dia:"Seg", semanas:[3,1,4,2,…] }, … ]  (5 dias × 16 semanas)
        [HttpGet("atividade")]
        public async Task<IActionResult> GetAtividade()
        {
            var dados = await _relatoriosService.GetAtividadeAsync();
            return Ok(new ApiResponse<List<AtividadeHeatmapDTO>>(true, "Atividade por dia", dados));
        }
    }
}
