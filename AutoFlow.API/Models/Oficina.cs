using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AutoFlow.API.Models
{
    [Table("Oficinas")]
    public class Oficina
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(150)]
        public string Nome { get; set; } = "";
        [Required, MaxLength(50)]
        public string Slug { get; set; } = "";
        [MaxLength(18)]
        public string? Cnpj { get; set; }
        [Required, MaxLength(150)]
        public string Email { get; set; } = "";
        [MaxLength(20)]
        public string? Telefone { get; set; }
        public Guid? PlanoId { get; set; }
        [ForeignKey("PlanoId")]
        [JsonIgnore] // Evita serialização do objeto Plano completo nas respostas (React Error #31)
        public Plano? Plano { get; set; }
        [MaxLength(20)]
        public string Status { get; set; } = "trial";
        public DateTime? TrialAte { get; set; }
        [Required, MaxLength(60)]
        public string SchemaName { get; set; } = "";
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
