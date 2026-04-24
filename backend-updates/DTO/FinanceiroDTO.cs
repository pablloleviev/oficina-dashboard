namespace AutoFlow.API.DTO
{
    // ── GET /api/financeiro/resumo ──────────────────────────────────────────
    public class FinanceiroResumoDTO
    {
        public decimal LucroLiquido { get; set; }
        public decimal ReceitaBruta { get; set; }
        public decimal AReceber { get; set; }
        public decimal Despesas { get; set; }          // 0 até entidade Despesa existir
        public double VariacaoLucro { get; set; }      // % vs período anterior
        public double VariacaoReceita { get; set; }
        public double VariacaoDespesas { get; set; }
        public int OrdensAReceber { get; set; }        // qtd de OS pendentes
    }

    // ── GET /api/financeiro/receita-semanal ─────────────────────────────────
    public class ReceitaSemanalDTO
    {
        public string Semana { get; set; } = string.Empty;  // "S1", "S2", "S3", "S4"
        public decimal Receita { get; set; }
        public decimal Despesa { get; set; }
    }

    // ── GET /api/financeiro/por-servico ─────────────────────────────────────
    public class PorServicoDTO
    {
        public string Nome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public double Percentual { get; set; }
    }

    // ── GET /api/financeiro/transacoes ──────────────────────────────────────
    public class TransacaoDTO
    {
        // "entrada" = OS faturada | "saida" = despesa | "pendente" = OS não faturada
        public string Tipo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
        // "Recebido" | "Pendente" | "Pago"
        public string Status { get; set; } = string.Empty;
    }
}
