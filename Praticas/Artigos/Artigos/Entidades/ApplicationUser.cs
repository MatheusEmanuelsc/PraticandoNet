using Microsoft.AspNetCore.Identity;

namespace Artigos.Entidades;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = String.Empty;
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public bool IsActive { get; set; } = true;

   
    public ICollection<Article>? Articles { get; set; }
}