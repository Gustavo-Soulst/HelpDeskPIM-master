using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "O campo Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O Email não é válido.")]
    public string Email { get; set; }

    [Required]
    public string SenhaHash { get; set; }

    // Propriedade de navegação para os chamados que este usuário criou
    public ICollection<Chamado> Chamados { get; set; } = new List<Chamado>();
}
