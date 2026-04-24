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
    public class FinanceiroController : ControllerBase
    {
        private readonly FinanceiroService _financeiroService;

        public FinanceiroController(FinanceiroService financeiroService)
        {
            _financeiroService = financeiroService;
        }

        // GET /api/financeiro/resumo?periodo=Este%20mês
        // Query param: periodo  →  "7 dias" | "Este mês" | "3 meses" | "Ano"
        // Response:
        // {
        //   lucroLiquido, receitaBruta, aReceber, despesas,
        //   variacaoLucro, variacaoReceita, variacaoDespesas, ordensAReceber
        // }
        [HttpGet("resumo")]
        public async Task<IActionResult> GetResumo([FromQuery] string periodo = "Este mês")
        {
            var resumo = await _financeiroService.GetResumoAsync(periodo);
            return Ok(new ApiResponse<FinanceiroResumoDTO>(true, "Resumo financeiro", resumo));
        }

        // GET /api/financeiro/receita-semanal?periodo=Este%20mês
        // Response: [ { semana:"S1", receita:520, despesa:0 }, … ]
        [HttpGet("receita-semanal")]
        public async Task<IActionResult> GetReceitaSemanal([FromQuery] string periodo = "Este mês")
        {
            var dados = await _financeiroService.GetReceitaSemanalAsync(periodo);
            return Ok(new ApiResponse<List<ReceitaSemanalDTO>>(true, "Receita semanal", dados));
        }

        // GET /api/financeiro/por-servico?periodo=Este%20mês
        // Response: [ { nome:"Troca de Óleo", valor:750, percentual:30.6 }, … ]
        [HttpGet("por-servico")]
        public async Task<IActionResult> GetPorServico([FromQuery] string periodo = "Este mês")
        {
            var dados = await _financeiroService.GetPorServicoAsync(periodo);
            return Ok(new ApiResponse<List<PorServicoDTO>>(true, "Receita por serviço", dados));
        }

        // GET /api/financeiro/transacoes?periodo=Este%20mês
        // Response: [ { tipo:"entrada", descricao, cliente, data, valor, status }, … ]
        [HttpGet("transacoes")]
        public async Task<IActionResult> GetTransacoes([FromQuery] string periodo = "Este mês")
        {
            var transacoes = await _financeiroService.GetTransacoesAsync(periodo);
            return Ok(new ApiResponse<List<TransacaoDTO>>(true, "Transações", transacoes));
        }
    }
}
