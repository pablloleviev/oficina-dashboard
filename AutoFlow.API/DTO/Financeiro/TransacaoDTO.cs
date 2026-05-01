using System;

namespace AutoFlow.API.DTO.Financeiro
{
    public class TransacaoDTO
    {
        public string Tipo { get; set; } = string.Empty; // "Receita" ou "Despesa"
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string MeioPagamento { get; set; } = string.Empty;
        
        public int ReferenciaId { get; set; } // ID da OS ou ID da Despesa
    }
}
