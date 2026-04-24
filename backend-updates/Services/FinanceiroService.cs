using AutoFlow.API.Data;
using AutoFlow.API.DTO;
using AutoFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow.API.Services
{
    public class FinanceiroService
    {
        private readonly AppDbContext _db;

        public FinanceiroService(AppDbContext db)
        {
            _db = db;
        }

        // ── Converte string de período em intervalo de datas ─────────────────
        private static (DateTime inicio, DateTime fim) ResolvePeriodo(string periodo)
        {
            var fim = DateTime.UtcNow;
            var inicio = periodo switch
            {
                "7 dias" => fim.AddDays(-7),
                "3 meses" => fim.AddMonths(-3),
                "Ano" => fim.AddYears(-1),
                _ => new DateTime(fim.Year, fim.Month, 1)   // "Este mês" (default)
            };
            return (inicio, fim);
        }

        // ── GET /api/financeiro/resumo ───────────────────────────────────────
        public async Task<FinanceiroResumoDTO> GetResumoAsync(string periodo)
        {
            var (inicio, fim) = ResolvePeriodo(periodo);
            var duracao = fim - inicio;
            var inicioPrev = inicio - duracao;

            // Período atual
            var ordensPeriodo = await _db.OrdensServico
                .Where(o => o.DataFaturamento >= inicio && o.DataFaturamento <= fim)
                .ToListAsync();

            var faturadas = ordensPeriodo.Where(o => o.Faturado).ToList();
            var pendentes = ordensPeriodo.Where(o =>
                !o.Faturado &&
                (o.Status == StatusOrdemServico.Finalizado ||
                 o.Status == StatusOrdemServico.Entregue)).ToList();

            var receitaBruta = faturadas.Sum(o => o.Valor);

            // Período anterior (para variação %)
            var receitaAnterior = await _db.OrdensServico
                .Where(o => o.Faturado &&
                            o.DataFaturamento >= inicioPrev &&
                            o.DataFaturamento < inicio)
                .SumAsync(o => (decimal?)o.Valor) ?? 0m;

            var variacaoReceita = receitaAnterior > 0
                ? Math.Round((double)((receitaBruta - receitaAnterior) / receitaAnterior * 100), 1)
                : 0;

            return new FinanceiroResumoDTO
            {
                ReceitaBruta = receitaBruta,
                LucroLiquido = receitaBruta,        // sem despesas por enquanto
                AReceber = pendentes.Sum(o => o.Valor),
                Despesas = 0m,                      // expandir quando entidade Despesa existir
                VariacaoReceita = variacaoReceita,
                VariacaoLucro = variacaoReceita,
                VariacaoDespesas = 0,
                OrdensAReceber = pendentes.Count
            };
        }

        // ── GET /api/financeiro/receita-semanal ──────────────────────────────
        // Divide o período em 4 semanas e retorna receita por semana
        public async Task<List<ReceitaSemanalDTO>> GetReceitaSemanalAsync(string periodo)
        {
            var (inicio, fim) = ResolvePeriodo(periodo);

            var ordens = await _db.OrdensServico
                .Where(o => o.Faturado &&
                            o.DataFaturamento >= inicio &&
                            o.DataFaturamento <= fim)
                .ToListAsync();

            var totalDias = (fim - inicio).TotalDays;
            var semanaEmDias = totalDias / 4;
            var result = new List<ReceitaSemanalDTO>();

            for (int i = 0; i < 4; i++)
            {
                var semanaInicio = inicio.AddDays(i * semanaEmDias);
                var semanaFim = inicio.AddDays((i + 1) * semanaEmDias);

                result.Add(new ReceitaSemanalDTO
                {
                    Semana = $"S{i + 1}",
                    Receita = ordens
                        .Where(o => o.DataFaturamento >= semanaInicio &&
                                    o.DataFaturamento < semanaFim)
                        .Sum(o => o.Valor),
                    Despesa = 0m   // expandir com entidade Despesa
                });
            }

            return result;
        }

        // ── GET /api/financeiro/por-servico ──────────────────────────────────
        // Top 5 serviços por receita faturada no período
        public async Task<List<PorServicoDTO>> GetPorServicoAsync(string periodo)
        {
            var (inicio, fim) = ResolvePeriodo(periodo);

            // Agrupa em memória (evita problemas de tradução EF com GroupBy + Sum)
            var raw = await _db.OrdensServico
                .Where(o => o.Faturado &&
                            o.DataFaturamento >= inicio &&
                            o.DataFaturamento <= fim)
                .Select(o => new { o.Servico, o.Valor })
                .ToListAsync();

            var agrupado = raw
                .GroupBy(o => o.Servico)
                .Select(g => new { Nome = g.Key, Valor = g.Sum(x => x.Valor) })
                .OrderByDescending(x => x.Valor)
                .Take(5)
                .ToList();

            var total = agrupado.Sum(x => x.Valor);

            return agrupado.Select(x => new PorServicoDTO
            {
                Nome = x.Nome,
                Valor = x.Valor,
                Percentual = total > 0
                    ? Math.Round((double)(x.Valor / total * 100), 1)
                    : 0
            }).ToList();
        }

        // ── GET /api/financeiro/transacoes ───────────────────────────────────
        // OS faturadas = "entrada" · OS pendentes de faturamento = "pendente"
        public async Task<List<TransacaoDTO>> GetTransacoesAsync(string periodo)
        {
            var (inicio, fim) = ResolvePeriodo(periodo);

            var ordens = await _db.OrdensServico
                .Where(o =>
                    // faturadas no período
                    (o.Faturado && o.DataFaturamento >= inicio && o.DataFaturamento <= fim)
                    ||
                    // pendentes de faturamento (sem filtro de data)
                    (!o.Faturado && (
                        o.Status == StatusOrdemServico.Finalizado ||
                        o.Status == StatusOrdemServico.Entregue)))
                .OrderByDescending(o => o.DataFaturamento)
                .Take(30)
                .ToListAsync();

            return ordens.Select(o => new TransacaoDTO
            {
                Tipo = o.Faturado ? "entrada" : "pendente",
                Descricao = o.Servico,
                Cliente = o.Cliente,
                Data = o.DataFaturamento ?? DateTime.UtcNow,
                Valor = o.Valor,
                Status = o.Faturado ? "Recebido" : "Pendente"
            }).ToList();
        }
    }
}
