namespace AutoFlow.API.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Documento { get; set; } // CPF ou CNPJ (string simples)

        // Isolamento por usuário — igual a Servico e OrdemServico
        public int UsuarioId { get; set; }

        // MULTI-TENANT: a qual oficina este registro pertence.
        public Guid? OficinaId { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        // ========================= SOFT DELETE =========================
        public bool IsActive { get; set; } = true;

        // ========================= RELACIONAMENTOS =========================
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}
