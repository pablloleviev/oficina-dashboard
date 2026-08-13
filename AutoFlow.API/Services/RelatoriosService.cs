using AutoFlow.API.Data;
using AutoFlow.API.DTO.Relatorios;
using AutoFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFlow.API.Services
{
    public class RelatoriosService
    {
        private readonly AppDbContext _context;

        public RelatoriosService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EvolucaoFaturamentoDTO>> GetEvolucaoFaturamento(int userId)
        {
            // Pega receitas dos últimos 12 meses
            var dataCorte = DateTime.UtcNow.AddMonths(-12);

            var ordens = await _context.OrdemServicos
                .Where(os => os.DataCriacao >= dataCorte &&
                             (os.Status == StatusOrdemServico.Entregue || 
                              os.Status == StatusOrdemServico.Finalizado || 
                              os.Faturado))
                .Select(os => new { os.DataCriacao, os.Valor }) // Projeção antes do ToList
                .ToListAsync();

            if (!ordens.Any()) return new List<EvolucaoFaturamentoDTO>();

            return ordens
                .GroupBy(os => os.DataCriacao.ToString("MM/yyyy"))
                .Select(g => new EvolucaoFaturamentoDTO
                {
                    Mes = g.Key,
                    Total = g.Sum(os => os.Valor)
                })
                .OrderBy(x => x.Mes)
                .ToList();
        }

        public async Task<List<TopClienteDTO>> GetTopClientes(int userId, int limite = 5)
        {
            var ordens = await _context.OrdemServicos
                .Where(os => (os.Status == StatusOrdemServico.Entregue || 
                              os.Status == StatusOrdemServico.Finalizado || 
                              os.Faturado))
                .Select(os => new { os.ClienteId, Nome = (os.Cliente != null ? os.Cliente.Nome : "Cliente Desconhecido"), os.Valor })
                .ToListAsync();

            if (!ordens.Any()) return new List<TopClienteDTO>();

            return ordens
                .GroupBy(os => new { os.ClienteId, os.Nome })
                .Select(g => new TopClienteDTO
                {
                    ClienteId = g.Key.ClienteId,
                    Nome = g.Key.Nome,
                    TotalGasto = g.Sum(os => os.Valor),
                    QuantidadeOS = g.Count()
                })
                .OrderByDescending(x => x.TotalGasto)
                .Take(limite)
                .ToList();
        }

        public async Task<List<TopServicoDTO>> GetTopServicos(int userId, int limite = 5)
        {
            var ordens = await _context.OrdemServicos
                .Where(os => (os.Status == StatusOrdemServico.Entregue || 
                              os.Status == StatusOrdemServico.Finalizado || 
                              os.Faturado))
                .Select(os => new { os.Servico, os.Valor })
                .ToListAsync();

            if (!ordens.Any()) return new List<TopServicoDTO>();

            return ordens
                .GroupBy(os => os.Servico ?? "Não Informado")
                .Select(g => new TopServicoDTO
                {
                    Servico = g.Key,
                    Frequencia = g.Count(),
                    ReceitaGerada = g.Sum(os => os.Valor)
                })
                .OrderByDescending(x => x.Frequencia)
                .Take(limite)
                .ToList();
        }

        public async Task<List<AtividadeHeatmapDTO>> GetAtividadeHeatmap(int userId, int diasBack = 30)
        {
            var dataCorte = DateTime.UtcNow.AddDays(-diasBack);

            var ordens = await _context.OrdemServicos
                .Where(os => os.DataCriacao >= dataCorte)
                .Select(os => new { os.DataCriacao })
                .ToListAsync();

            if (!ordens.Any()) return new List<AtividadeHeatmapDTO>();

            return ordens
                .GroupBy(os => os.DataCriacao.ToString("yyyy-MM-dd"))
                .Select(g => new AtividadeHeatmapDTO
                {
                    Data = g.Key,
                    Volume = g.Count()
                })
                .OrderBy(x => x.Data)
                .ToList();
        }

        public async Task<DashboardStatsDTO> GetDashboardStats(int userId)
        {
            // Clientes Ativos
            var totalClientes = await _context.Clientes
                .CountAsync(c => c.IsActive);

            // Veículos Vinculados
            var totalVeiculos = await _context.Veiculos
                .CountAsync(v => v.IsActive && _context.Clientes.Any(c => c.Id == v.ClienteId));

            // Ordens Pendentes (Não faturadas/Finalizadas)
            var ordensPendentes = await _context.OrdemServicos
                .CountAsync(os => os.Status == StatusOrdemServico.Pendente);

            // Ticket Médio: Soma OS Pagas / Clientes que pagaram
            var dadosPagos = await _context.OrdemServicos
                .Where(os => (os.Status == StatusOrdemServico.Entregue || 
                              os.Status == StatusOrdemServico.Finalizado || 
                              os.Faturado))
                .Select(os => new { os.ClienteId, os.Valor })
                .ToListAsync();

            decimal ticketMedio = 0;
            if (dadosPagos.Any())
            {
                var somaTotal = dadosPagos.Sum(x => x.Valor);
                var clientesUnicos = dadosPagos.Select(x => x.ClienteId).Distinct().Count();

                // 🔥 PROTEÇÃO EXTRA CONTRA DIVISÃO POR ZERO
                if (clientesUnicos > 0)
                {
                    ticketMedio = somaTotal / clientesUnicos;
                }
            }

            return new DashboardStatsDTO
            {
                TotalClientes = totalClientes,
                TotalVeiculos = totalVeiculos,
                OrdensPendentes = ordensPendentes,
                TicketMedio = ticketMedio
            };
        }
    }
}
