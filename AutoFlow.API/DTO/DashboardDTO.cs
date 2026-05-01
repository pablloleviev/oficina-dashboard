namespace AutoFlow.API.DTO
{
    public class DashboardDTO
    {
        public int TotalOrdens { get; set; }
        public decimal FaturamentoTotal { get; set; }
        public decimal FaturamentoFinalizado { get; set; }

        public int Pendentes { get; set; }
        public int EmAndamento { get; set; }
        public int Finalizados { get; set; }
        public int Entregues { get; set; }
    }
}
