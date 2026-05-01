namespace AutoFlow.API.DTO.Relatorios
{
    public class EvolucaoFaturamentoDTO
    {
        public string Mes { get; set; } = string.Empty; 
        public decimal Total { get; set; }
    }

    public class TopClienteDTO
    {
        public int ClienteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal TotalGasto { get; set; }
        public int QuantidadeOS { get; set; }
    }

    public class TopServicoDTO
    {
        public string Servico { get; set; } = string.Empty;
        public int Frequencia { get; set; }
        public decimal ReceitaGerada { get; set; }
    }

    public class AtividadeHeatmapDTO
    {
        public string Data { get; set; } = string.Empty; 
        public int Volume { get; set; }
    }

    public class DashboardStatsDTO
    {
        public int TotalClientes { get; set; }
        public int TotalVeiculos { get; set; }
        public int OrdensPendentes { get; set; }
        public decimal TicketMedio { get; set; }
    }
}
