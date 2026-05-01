namespace AutoFlow.API.DTO
{
    public class OrdemServicoDTO
    {
        public int? ClienteId { get; set; }
        public int? VeiculoId { get; set; }
        public string Servico { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; }
        public string MeioPagamento { get; set; }
    }
}
