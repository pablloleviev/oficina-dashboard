namespace AutoFlow.API.DTO
{
    // ── GET /api/relatorios/resumo ──────────────────────────────────────────
    // Hero card + 4 métricas
    public class RelatorioResumoDTO
    {
        // Hero card
        public double CrescimentoMensal { get; set; }   // ex: 24.0 → exibe "+24%"
        public decimal Faturamento { get; set; }
        public int OrdensNoMes { get; set; }
        public int ClientesAtivos { get; set; }

        // Métricas avançadas
        public double TaxaConclusao { get; set; }       // % de OS finalizadas/entregues
        public double TempoMedioOsDias { get; set; }    // média de dias abertura→entrega
        public double RetornoClientes { get; set; }     // % clientes que voltaram
    }

    // ── GET /api/relatorios/evolucao-faturamento ────────────────────────────
    // 6 pontos para o gráfico de área (ano atual vs ano anterior)
    public class EvolucaoFaturamentoDTO
    {
        public string Mes { get; set; } = string.Empty;    // "Nov", "Dez", "Jan" …
        public decimal ValorAtual { get; set; }
        public decimal ValorAnterior { get; set; }
    }

    // ── GET /api/relatorios/top-clientes ────────────────────────────────────
    public class TopClienteDTO
    {
        public int Posicao { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int TotalOrdens { get; set; }
        public decimal ValorTotal { get; set; }
    }

    // ── GET /api/relatorios/top-servicos ────────────────────────────────────
    public class TopServicoDTO
    {
        public int Posicao { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Frequencia { get; set; }
        public decimal ValorTotal { get; set; }
    }

    // ── GET /api/relatorios/atividade ───────────────────────────────────────
    // Heatmap: 5 dias (Seg–Sex) × N semanas (contagem de OS por célula)
    public class AtividadeHeatmapDTO
    {
        public string Dia { get; set; } = string.Empty;   // "Seg", "Ter", …
        public List<int> Semanas { get; set; } = new();   // cada int = qtd OS naquela semana
    }
}
