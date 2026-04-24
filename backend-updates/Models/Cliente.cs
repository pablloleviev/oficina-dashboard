namespace AutoFlow.API.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;

        // Navigation
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}
