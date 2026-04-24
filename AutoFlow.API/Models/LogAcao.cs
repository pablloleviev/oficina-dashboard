namespace AutoFlow.API.Models
{
    public class LogAcao
    {
        public int Id { get; set; }

        public int OrdemServicoId { get; set; }

        public int UsuarioId { get; set; }

        public string Acao { get; set; } // FATURAR, DESFATURAR

        public DateTime Data { get; set; } = DateTime.UtcNow;
    }
}