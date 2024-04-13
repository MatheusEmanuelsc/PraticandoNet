

### UserRegistro.cs

```csharp
namespace _02_DtoAutoMapper.Dtos
{
    public class UserRegistro
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        
        [Required]
        public string? UserName { get; set; }
        
        [Required]
        [StringLength(8)]
        public string? Password { get; set; }
    }
}
```

### User.cs

```csharp
namespace _02_DtoAutoMapper.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public User(UserRegistro userRegistro)
        {
            UserName = userRegistro.UserName;
            Email = userRegistro.Email;
            Password = userRegistro.Password;
        }
    }
}

