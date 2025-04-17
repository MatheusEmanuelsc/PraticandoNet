using Microsoft.AspNetCore.Identity;

namespace ListaTarefas.Api.Entities;

public class ApplicationUser : IdentityUser

{
    public string FullName { get; set; } = string.Empty;
}