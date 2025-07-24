namespace Decolei.net.DTOs // ou o namespace correto do seu projeto
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Documento { get; set; }
        public string Perfil { get; set; } // Vai conter a "Role" (ADMIN, CLIENTE, etc.)
    }
}