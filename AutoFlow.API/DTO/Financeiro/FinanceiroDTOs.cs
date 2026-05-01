using System;

namespace AutoFlow.API.DTO.Financeiro
{
    public class DespesaDTO
    {
        public int? Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        
        public string Categoria { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string MeioPagamento { get; set; } = string.Empty;
        public int? OrdemServicoId { get; set; }
    }

    public class ResumoFinanceiroDTO
    {
        public decimal TotalReceitas { get; set; } // Entregue, Finalizado ou Faturado
        public decimal TotalDespesas { get; set; } // Pagas
        public decimal Saldo => TotalReceitas - TotalDespesas;
        
        public int OrdensPendentes { get; set; }
        public int OrdensEmAndamento { get; set; }
    }
}
