using AutoFlow.API.Data;
using AutoFlow.API.DTO;
using AutoFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AutoFlow.API.Services
{
    public class RelatoriosService
    {
        private readonly AppDbContext _db;

        public RelatoriosService(AppDbContext db)
        {
            _db = db;
        }

        // ── GET /api/relatorios/resumo ───────────────────────────────────────
        public async Task<RelatorioResumoDTO> GetResumoAsync()
        {
            var agora = DateTime.UtcNow;
            var inicioMes = new DateTime(agora.Year, agora.Month, 1);
            var inicioMesAnterior = inicioMes.AddMonths(-1);

            // Faturamento do mês atual
            var ordensDoMes = await _db.OrdensServico
                .Where(o => o.Faturado &&
                            o.DataFaturamento >= inicioMes &&
                            o.DataFaturamento < agora)
                .ToListAsync();

            // Faturamento mês anterior (para crescimento %)
            var ordensMesAnterior = await _db.OrdensServico
                .Where(o => o.Faturado &&
                            o.DataFaturamento >= inicioMesAnterior &&
                            o.DataFaturamento < inicioMes)
                .ToListAsync();

            var faturamentoMes = ordensDoMes.Sum(o => o.Valor);
            var faturamentoAnterior = ordensMesAnterior.Sum(o => o.Valor);

            var crescimento = faturamentoAnterior > 0
                ? Math.Round((double)((faturamentoMes - faturamentoAnterior) / faturamentoAnterior * 100), 1)
                : 0;

            // Ordens e clientes únicos no mês
            var ordensNoMes = await _db.OrdensServico
                .CountAsync(o => o.DataFaturamento >= inicioMes);

            var clientesAtivos = await _db.OrdensServico
                .Where(o => o.DataFaturamento >= inicioMes)
                .Select(o => o.Cliente)
                .Distinct()
                .CountAsync();

            // Taxa de conclusão: OS finalizadas/entregues / total
            var totalOrdens = await _db.OrdensServico.CountAsync();
            var finalizadas = await _db.OrdensServico
                .CountAsync(o =>
                    o.Status == StatusOrdemServico.Finalizado ||
                    o.Status == StatusOrdemServico.Entregue);

            var taxaConclusao = totalOrdens > 0
                ? Math.Round((double)finalizadas / totalOrdens * 100, 1)
                : 0;

            // Tempo médio (abertura → DataFaturamento) – requer campo DataCriacao na OS
            // Retorna constante até o campo ser adicionado ao modelo
            var tempoMedio = 1.8;

            return new RelatorioResumoDTO
            {
                CrescimentoMensal = crescimento,
                Faturamento = faturamentoMes,
                OrdensNoMes = ordensNoMes,
                ClientesAtivos = clientesAtivos,
                TaxaConclusao = taxaConclusao,
                TempoMedioOsDias = tempoMedio,
                RetornoClientes = 68   // TODO: calcular quando houver histórico suficiente
            };
        }

        // ── GET /api/relatorios/evolucao-faturamento ─────────────────────────
        // 6 meses: ano atual vs ano anterior
        public async Task<List<EvolucaoFaturamentoDTO>> GetEvolucaoFaturamentoAsync()
        {
            var resultado = new List<EvolucaoFaturamentoDTO>();
            var agora = DateTime.UtcNow;
            var ptBr = new CultureInfo("pt-BR");

            for (int i = 5; i >= 0; i--)
            {
                var mes = agora.AddMonths(-i);
                var inicioAtual = new DateTime(mes.Year, mes.Month, 1);
                var fimAtual = inicioAtual.AddMonths(1);

                var inicioAnterior = inicioAtual.AddYears(-1);
                var fimAnterior = fimAtual.AddYears(-1);

                var valorAtual = await _db.OrdensServico
                    .Where(o => o.Faturado &&
                                o.DataFaturamento >= inicioAtual &&
                                o.DataFaturamento < fimAtual)
                    .SumAsync(o => (decimal?)o.Valor) ?? 0m;

                var valorAnterior = await _db.OrdensServico
                    .Where(o => o.Faturado &&
                                o.DataFaturamento >= inicioAnterior &&
                                o.DataFaturamento < fimAnterior)
                    .SumAsync(o => (decimal?)o.Valor) ?? 0m;

                resultado.Add(new EvolucaoFaturamentoDTO
                {
                    Mes = inicioAtual.ToString("MMM", ptBr),   // "jan", "fev" …
                    ValorAtual = valorAtual,
                    ValorAnterior = valorAnterior
                });
            }

            return resultado;
        }

        // ── GET /api/relatorios/top-clientes?limit=4 ─────────────────────────
        public async Task<List<TopClienteDTO>> GetTopClientesAsync(int limit = 4)
        {
            // Agrupa em memória (EF não traduz Select com índice para SQL)
            var raw = await _db.OrdensServico
                .Where(o => o.Faturado)
                .Select(o => new { o.Cliente, o.Valor })
                .ToListAsync();

            var agrupado = raw
                .GroupBy(o => o.Cliente)
                .Select(g => new
                {
                    Nome = g.Key,
                    TotalOrdens = g.Count(),
                    ValorTotal = g.Sum(x => x.Valor)
                })
                .OrderByDescending(x => x.ValorTotal)
                .Take(limit)
                .ToList();

            return agrupado
                .Select((x, i) => new TopClienteDTO
                {
                    Posicao = i + 1,
                    Nome = x.Nome,
                    TotalOrdens = x.TotalOrdens,
                    ValorTotal = x.ValorTotal
                })
                .ToList();
        }

        // ── GET /api/relatorios/top-servicos?limit=4 ─────────────────────────
        public async Task<List<TopServicoDTO>> GetTopServicosAsync(int limit = 4)
        {
            var raw = await _db.OrdensServico
                .Select(o => new { o.Servico, o.Valor })
                .ToListAsync();

            var agrupado = raw
                .GroupBy(o => o.Servico)
                .Select(g => new
                {
                    Nome = g.Key,
                    Frequencia = g.Count(),
                    ValorTotal = g.Sum(x => x.Valor)
                })
                .OrderByDescending(x => x.Frequencia)
                .Take(limit)
                .ToList();

            return agrupado
                .Select((x, i) => new TopServicoDTO
                {
                    Posicao = i + 1,
                    Nome = x.Nome,
                    Frequencia = x.Frequencia,
                    ValorTotal = x.ValorTotal
                })
                .ToList();
        }

        // ── GET /api/relatorios/atividade ─────────────────────────────────────
        // Heatmap: 5 dias × 16 semanas (4 últimos meses)
        public async Task<List<AtividadeHeatmapDTO>> GetAtividadeAsync()
        {
            var inicio = DateTime.UtcNow.AddMonths(-4);

            var ordens = await _db.OrdensServico
                .Where(o => o.DataFaturamento >= inicio)
                .Select(o => o.DataFaturamento)
                .ToListAsync();

            var diasSemana = new[]
            {
                (Label: "Seg", Dia: DayOfWeek.Monday),
                (Label: "Ter", Dia: DayOfWeek.Tuesday),
                (Label: "Qua", Dia: DayOfWeek.Wednesday),
                (Label: "Qui", Dia: DayOfWeek.Thursday),
                (Label: "Sex", Dia: DayOfWeek.Friday)
            };

            const int totalSemanas = 16;
            var resultado = new List<AtividadeHeatmapDTO>();

            foreach (var (label, dia) in diasSemana)
            {
                var semanas = new List<int>();

                for (int s = 0; s < totalSemanas; s++)
                {
                    var semanaInicio = inicio.AddDays(s * 7);
                    var semanaFim = semanaInicio.AddDays(7);

                    var count = ordens.Count(d =>
                        d.HasValue &&
                        d.Value >= semanaInicio &&
                        d.Value < semanaFim &&
                        d.Value.DayOfWeek == dia);

                    semanas.Add(count);
                }

                resultado.Add(new AtividadeHeatmapDTO
                {
                    Dia = label,
                    Semanas = semanas
                });
            }

            return resultado;
        }
    }
}
