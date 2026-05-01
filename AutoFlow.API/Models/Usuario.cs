namespace AutoFlow.API.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Senha { get; set; } = string.Empty;

        // 🔥 NOVO: CONTROLE DE PERMISSÃO
        public string Role { get; set; } = "User"; // User | Admin
    }
}
