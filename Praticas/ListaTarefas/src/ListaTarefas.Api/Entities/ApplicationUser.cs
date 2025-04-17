using Microsoft.AspNetCore.Identity;

namespace ListaTarefas.Api.Entities;

public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty; // Campo personalizado para nome completo
}