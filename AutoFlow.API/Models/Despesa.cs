using System;

namespace AutoFlow.API.Models
{
    public enum CategoriaDespesa
    {
        Geral = 0,
        Aluguel = 1,
        Pecas = 2,
        Marketing = 3,
        FolhaPagamento = 4,
        Impostos = 5
    }

    public enum StatusDespesa
    {
        Pendente = 0,
        Pago = 1,
        Cancelado = 2
    }

    public class Despesa
    {
        public int Id { get; set; }

        public string Descricao { get; set; } = string.Empty;
        
        public decimal Valor { get; set; }
        
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        
        public CategoriaDespesa Categoria { get; set; }
        public StatusDespesa Status { get; set; }
        public MeioPagamento? MeioPagamento { get; set; }

        public int? OrdemServicoId { get; set; }
        public OrdemServico? OrdemServico { get; set; }

        // Mapeamento por usuário igual ao restante do sistema
        public int UsuarioId { get; set; } 
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
