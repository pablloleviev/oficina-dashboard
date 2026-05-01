using AutoFlow.API.Models;

namespace AutoFlow.API.DTO
{
    public class OrdemServicoResponseDTO
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;

        public int VeiculoId { get; set; }
        public string Veiculo { get; set; } = string.Empty; // Nome do modelo/marca
        public string Placa { get; set; } = string.Empty;

        public string Servico { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MeioPagamento { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime? DataConclusao { get; set; }

        public bool Faturado { get; set; }
        public DateTime? DataFaturamento { get; set; }
    }
}
