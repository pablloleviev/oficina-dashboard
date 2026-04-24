namespace AutoFlow.API.DTO
{
    public class ServicoDTO
    {
        public string Cliente { get; set; } = string.Empty;

        public string Veiculo { get; set; } = string.Empty;

        public string Placa { get; set; } = string.Empty;

        public string NomeServico { get; set; } = string.Empty;

        public decimal Valor { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
