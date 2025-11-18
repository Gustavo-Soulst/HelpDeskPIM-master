using System;
using System.ComponentModel.DataAnnotations;

public class Chamado
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O campo Título é obrigatório.")]
    public string Titulo { get; set; }

    public string Descricao { get; set; }

    public DateTime DataAbertura { get; set; } = DateTime.Now;

    public string Status { get; set; } = "Aberto";

    // Chave estrangeira para o usuário que abriu o chamado
    public int UsuarioId { get; set; }

    // Propriedade de navegação para o usuário
    public Usuario? Usuario { get; set; }
}
