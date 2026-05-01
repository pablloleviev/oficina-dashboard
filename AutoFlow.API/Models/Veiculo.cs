namespace AutoFlow.API.Models
{
    public class Veiculo
    {
        public int Id { get; set; }
        
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public int Ano { get; set; }

        // Chave Estrangeira - 1:N
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
