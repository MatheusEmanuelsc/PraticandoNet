using Microsoft.AspNetCore.Identity;

namespace Artigos.Entidades;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = String.Empty;
    public bool IsActive { get; set; } = true;

   
    public ICollection<Article>? Articles { get; set; }
}