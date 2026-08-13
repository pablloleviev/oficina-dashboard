using AutoFlow.API.Models;

public class OrdemServico
{
    public int Id { get; set; }

    // ========================= RELACIONAMENTOS =========================
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int VeiculoId { get; set; }
    public Veiculo? Veiculo { get; set; }

    public string Servico { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public StatusOrdemServico Status { get; set; }
    public MeioPagamento? MeioPagamento { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataEntrada { get; set; } = DateTime.UtcNow;
    public DateTime? DataConclusao { get; set; }

    public int UsuarioId { get; set; }
    public Guid? OficinaId { get; set; }

    public bool Faturado { get; set; } = false;
    public DateTime? DataFaturamento { get; set; }

    public int? FaturadoPorUserId { get; set; }
    public int? DesfaturadoPorUserId { get; set; }
}
