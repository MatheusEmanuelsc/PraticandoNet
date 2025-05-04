using Microsoft.AspNetCore.Identity;

namespace Master.Persistence.Models;

public class AppUser : IdentityUser
{
    public string? NomeCompleto { get; set; }
    public string? Cargo { get; set; }
}