namespace AutoFlow.API.Models
{
    public class Veiculo
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Nome { get; set; } = string.Empty;   // ex: "Volkswagen Gol"
        public string Placa { get; set; } = string.Empty;  // ex: "ABC1234"
        public int Ano { get; set; }

        // Navigation
        public Cliente Cliente { get; set; } = null!;
    }
}
