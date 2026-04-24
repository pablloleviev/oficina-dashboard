namespace AutoFlow.API.DTO
{
    // ── Entrada: criar / atualizar cliente ──────────────────────────────────
    public class CreateClienteDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
    }

    // ── Entrada: adicionar veículo a um cliente ─────────────────────────────
    public class CreateVeiculoDTO
    {
        public string Nome { get; set; } = string.Empty;   // "Volkswagen Gol"
        public string Placa { get; set; } = string.Empty;  // "ABC1234"
        public int Ano { get; set; }
    }

    // ── Saída: item da listagem (tabela) ────────────────────────────────────
    public class ClienteListItemDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }
        public string VeiculoPrincipal { get; set; } = string.Empty;  // "Gol · ABC1234"
        public int TotalOrdens { get; set; }
        public decimal GastoTotal { get; set; }
    }

    // ── Saída: detalhe (drawer lateral) ────────────────────────────────────
    public class ClienteDetalheDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }
        public decimal TotalGasto { get; set; }
        public int OrdensAbertas { get; set; }
        public int OrdensFinalizadas { get; set; }
        public DateTime? UltimoServico { get; set; }
        public List<VeiculoDTO> Veiculos { get; set; } = new();
    }

    // ── Saída: veículo ──────────────────────────────────────────────────────
    public class VeiculoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public int Ano { get; set; }
    }

    // ── Saída: stats (3 cards do topo) ─────────────────────────────────────
    public class ClienteStatsDTO
    {
        public int TotalClientes { get; set; }
        public int ClientesAtivos { get; set; }
        public int VeiculosCadastrados { get; set; }
    }
}
