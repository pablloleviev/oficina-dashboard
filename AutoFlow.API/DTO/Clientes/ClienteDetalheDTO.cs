namespace AutoFlow.API.DTO.Clientes
{
    /// <summary>
    /// Projeção completa usada no detalhe GET /api/clientes/{id}.
    /// Inclui todos os campos do cliente.
    /// </summary>
    public class ClienteDetalheDTO
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Documento { get; set; }

        public DateTime CriadoEm { get; set; }

        public decimal TotalGasto { get; set; }

        public List<VeiculoDTO> Veiculos { get; set; } = new List<VeiculoDTO>();
    }
}
