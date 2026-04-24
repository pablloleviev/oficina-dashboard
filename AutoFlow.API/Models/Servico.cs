namespace AutoFlow.API.Models
{
    public class Servico
    {
        public int Id { get; set; }

        public string Cliente { get; set; } = string.Empty;

        public string Veiculo { get; set; } = string.Empty;

        public string Placa { get; set; } = string.Empty;

        public string NomeServico { get; set; } = string.Empty;

        public decimal Valor { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public int UsuarioId { get; set; }
        public int? FaturadoPorUserId { get; set; }
        public int? DesfaturadoPorUserId { get; set; }
    }
}
