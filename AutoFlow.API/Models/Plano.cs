using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoFlow.API.Models
{
    [Table("Planos")]
    public class Plano
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)]
        public string Nome { get; set; } = "";
        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }
        public int? LimiteOS { get; set; }
        public int LimiteUsuarios { get; set; } = 1;
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
