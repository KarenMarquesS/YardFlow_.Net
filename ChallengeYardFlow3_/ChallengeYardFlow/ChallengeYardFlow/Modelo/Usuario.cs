using System.ComponentModel.DataAnnotations;

namespace ChallengeYardFlow.Modelo
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Senha { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Funcao { get; set; } = string.Empty;

        
        public void AtualizaUsuario(int id) => Id = id;
    }
}
