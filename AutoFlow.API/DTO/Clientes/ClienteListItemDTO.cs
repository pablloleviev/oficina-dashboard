namespace AutoFlow.API.DTO.Clientes
{
    /// <summary>
    /// Projeção compacta usada na listagem GET /api/clientes.
    /// Retorna apenas campos necessários para exibição em grid.
    /// </summary>
    public class ClienteListItemDTO
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public string? Email { get; set; }

        public decimal TotalGasto { get; set; }
        
        public List<VeiculoDTO> Veiculos { get; set; } = new();
    }
}
