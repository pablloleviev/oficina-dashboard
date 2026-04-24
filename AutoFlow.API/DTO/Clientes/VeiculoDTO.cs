namespace AutoFlow.API.DTO.Clientes
{
    public class VeiculoDTO
    {
        public int? Id { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public int Ano { get; set; }
    }
}
