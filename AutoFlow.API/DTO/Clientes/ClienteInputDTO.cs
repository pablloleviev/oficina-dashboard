namespace AutoFlow.API.DTO.Clientes
{
    /// <summary>
    /// Payload de entrada para criação (POST) e atualização (PUT) de clientes.
    /// </summary>
    public class ClienteInputDTO
    {
        public string Nome { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Documento { get; set; } // CPF (11 dígitos) ou CNPJ (14 dígitos)

        public List<VeiculoDTO> Veiculos { get; set; } = new List<VeiculoDTO>();
    }
}
