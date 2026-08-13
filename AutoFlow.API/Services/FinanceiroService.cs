using AutoFlow.API.Data;
using AutoFlow.API.DTO.Financeiro;
using AutoFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFlow.API.Services
{
    public class FinanceiroService
    {
        private readonly AppDbContext _context;
        private readonly TenantProvider _tenant;

        public FinanceiroService(AppDbContext context, TenantProvider tenant)
        {
            _context = context;
            _tenant = tenant;
        }

        // ========================= RESUMO FINANCEIRO =========================
        public async Task<ResumoFinanceiroDTO> GetResumo(int userId)
        {
            // Regra: "Entregue ou Finalizada, contadas como Receita"
            var receitas = await _context.OrdemServicos
                .Where(os => os.Status == StatusOrdemServico.Finalizado || 
                             os.Status == StatusOrdemServico.Entregue || 
                             os.Faturado)
                .SumAsync(os => os.Valor);

            // Regra: Despesas Pagas
            var despesas = await _context.Despesas
                .Where(d => d.Status == StatusDespesa.Pago)
                .SumAsync(d => d.Valor);

            var ordensPendentes = await _context.OrdemServicos
                .CountAsync(os => os.Status == StatusOrdemServico.Pendente);

            var ordensEmAndamento = await _context.OrdemServicos
                .CountAsync(os => os.Status == StatusOrdemServico.EmAndamento);

            return new ResumoFinanceiroDTO
            {
                TotalReceitas = receitas,
                TotalDespesas = despesas,
                OrdensPendentes = ordensPendentes,
                OrdensEmAndamento = ordensEmAndamento
            };
        }

        // ========================= GET DESPESAS =========================
        public async Task<List<DespesaDTO>> GetDespesas(int userId)
        {
            return await _context.Despesas
                .OrderByDescending(d => d.DataVencimento)
                .Select(d => new DespesaDTO
                {
                    Id = d.Id,
                    Descricao = d.Descricao,
                    Valor = d.Valor,
                    DataVencimento = d.DataVencimento,
                    DataPagamento = d.DataPagamento,
                    Categoria = d.Categoria.ToString(),
                    Status = d.Status.ToString(),
                    MeioPagamento = d.MeioPagamento.ToString(),
                    OrdemServicoId = d.OrdemServicoId
                })
                .ToListAsync();
        }

        // ========================= CREATE DESPESA =========================
        public async Task<DespesaDTO> CreateDespesa(DespesaDTO dto, int userId)
        {
            if (!Enum.TryParse<CategoriaDespesa>(dto.Categoria, true, out var categoria))
                categoria = CategoriaDespesa.Geral;

            if (!Enum.TryParse<StatusDespesa>(dto.Status, true, out var status))
                status = StatusDespesa.Pendente;

            if (!Enum.TryParse<MeioPagamento>(dto.MeioPagamento, true, out var meioPagamento))
                meioPagamento = MeioPagamento.Desconhecido;

            var despesa = new Despesa
            {
                Descricao = dto.Descricao.Trim(),
                Valor = dto.Valor,
                DataVencimento = dto.DataVencimento,
                DataPagamento = status == StatusDespesa.Pago ? dto.DataPagamento ?? DateTime.UtcNow : null,
                Categoria = categoria,
                Status = status,
                MeioPagamento = status == StatusDespesa.Pago ? meioPagamento : null,
                OrdemServicoId = dto.OrdemServicoId,
                UsuarioId = userId,
                OficinaId = _tenant.OficinaId,
                DataCriacao = DateTime.UtcNow
            };

            _context.Despesas.Add(despesa);
            await _context.SaveChangesAsync();

            dto.Id = despesa.Id;
            return dto;
        }

        public async Task<DespesaDTO?> UpdateDespesa(int id, DespesaDTO dto, int userId)
        {
            var despesa = await _context.Despesas
                .FirstOrDefaultAsync(d => d.Id == id);

            if (despesa == null) return null;

            if (!Enum.TryParse<CategoriaDespesa>(dto.Categoria, true, out var categoria))
                categoria = despesa.Categoria;

            if (!Enum.TryParse<StatusDespesa>(dto.Status, true, out var status))
                status = despesa.Status;

            if (!Enum.TryParse<MeioPagamento>(dto.MeioPagamento, true, out var meioPagamento))
                meioPagamento = despesa.MeioPagamento ?? MeioPagamento.Desconhecido;

            despesa.Descricao = dto.Descricao.Trim();
            despesa.Valor = dto.Valor;
            despesa.DataVencimento = dto.DataVencimento;
            despesa.Categoria = categoria;
            despesa.Status = status;
            despesa.OrdemServicoId = dto.OrdemServicoId;

            // Lógica de pagamento
            if (status == StatusDespesa.Pago && despesa.DataPagamento == null)
            {
                despesa.DataPagamento = dto.DataPagamento ?? DateTime.UtcNow;
                despesa.MeioPagamento = meioPagamento;
            }
            else if (status != StatusDespesa.Pago)
            {
                despesa.DataPagamento = null;
                despesa.MeioPagamento = null;
            }
            else if (status == StatusDespesa.Pago)
            {
                despesa.MeioPagamento = meioPagamento;
            }

            await _context.SaveChangesAsync();

            dto.Id = despesa.Id;
            return dto;
        }

        public async Task<bool> DeleteDespesa(int id, int userId)
        {
            var despesa = await _context.Despesas
                .FirstOrDefaultAsync(d => d.Id == id);

            if (despesa == null) return false;

            _context.Despesas.Remove(despesa);
            await _context.SaveChangesAsync();

            return true;
        }

        // ========================= TRANSACOES =========================
        public async Task<List<TransacaoDTO>> GetTransacoes(int userId)
        {
            var ordens = await _context.OrdemServicos
                .Include(x => x.Cliente)
                .Where(os => os.Status == StatusOrdemServico.Entregue || os.Status == StatusOrdemServico.Finalizado || os.Faturado)
                .Select(os => new TransacaoDTO
                {
                    Tipo = "Receita",
                    Descricao = $"OS #{os.Id} - {os.Cliente.Nome}",
                    Valor = os.Valor,
                    Data = os.DataFaturamento ?? os.DataConclusao ?? os.DataCriacao,
                    Categoria = os.Servico,
                    Status = os.Status.ToString(),
                    MeioPagamento = os.MeioPagamento.ToString(),
                    ReferenciaId = os.Id
                }).ToListAsync();

            var despesas = await _context.Despesas
                .Select(d => new TransacaoDTO
                {
                    Tipo = "Despesa",
                    Descricao = d.Descricao,
                    Valor = d.Valor,
                    Data = d.DataPagamento ?? d.DataVencimento,
                    Categoria = d.Categoria.ToString(),
                    Status = d.Status.ToString(),
                    MeioPagamento = d.MeioPagamento.ToString(),
                    ReferenciaId = d.Id
                }).ToListAsync();

            return ordens.Concat(despesas)
                .OrderByDescending(t => t.Data)
                .ToList();
        }
    }
}
