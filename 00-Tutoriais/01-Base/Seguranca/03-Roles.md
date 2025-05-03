
# Próximos Passos: Implementação do ASP.NET Core Identity

## Implementação de Roles e Autorização Baseada em Papéis

Vamos implementar roles (papéis) para controlar o acesso às diferentes partes do seu aplicativo:

```csharp
// Seeds/RoleSeeder.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using YourNamespace.Models.Identity;

namespace YourNamespace.Seeds
{
    public static class RoleSeeder
    {
        public static async Task SeedRoles(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "Manager", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
```

Adicione a chamada para este seeder no `Program.cs`:

```csharp
// Program.cs (adicionando ao código existente)
var app = builder.Build();

// Adicionar inicialização de roles
if (app.Environment.IsDevelopment())
{
    // Executar o seeder ao iniciar a aplicação
    using (var scope = app.Services.CreateScope())
    {
        await RoleSeeder.SeedRoles(app.Services);
    }
}
```

## Configuração de Políticas de Autorização

Adicione políticas personalizadas para autorização mais granular:

```csharp
// Program.cs (adicionando à configuração de serviços)
builder.Services.AddAuthorization(options =>
{
    // Política baseada em role
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    
    // Política baseada em claim
    options.AddPolicy("CanManageUsers", policy => 
        policy.RequireClaim("Permission", "UserManagement"));
    
    // Política com múltiplos requisitos
    options.AddPolicy("SeniorStaff", policy => 
        policy.RequireRole("Admin", "Manager")
              .RequireClaim("EmploymentYears", "5", "10", "15", "20"));
});
```

## Serviço de Gerenciamento de Usuários

Crie um serviço para encapsular operações comuns de gerenciamento de usuários:

```csharp
// Services/UserService.cs
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourNamespace.Models.Identity;

namespace YourNamespace.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<bool> AssignRoleToUserAsync(string userId, string roleName);
        Task<IList<string>> GetUserRolesAsync(string userId);
    }

    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        
        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return _userManager.Users.ToList();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found");
                
            return await _userManager.DeleteAsync(user);
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            
            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);
                return result.Succeeded;
            }
            
            return true; // Usuário já possui o papel
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found");
                
            return await _userManager.GetRolesAsync(user);
        }
    }
}
```

Registre o serviço no container de DI:

```csharp
// Program.cs (adicionando à configuração de serviços)
builder.Services.AddScoped<IUserService, UserService>();
```

## Controller de Gerenciamento de Usuários

Crie um controller para gerenciar usuários:

```csharp
// Controllers/UserAdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YourNamespace.Models.Identity;
using YourNamespace.Services;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdminRole")]
    public class UserAdminController : ControllerBase
    {
        private readonly IUserService _userService;
        
        public UserAdminController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
                
            return Ok(user);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] ApplicationUser userModel)
        {
            if (id != userModel.Id)
                return BadRequest();
                
            var result = await _userService.UpdateUserAsync(userModel);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
                
            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
                
            return NoContent();
        }
        
        [HttpPost("{id}/roles/{role}")]
        public async Task<IActionResult> AssignRoleToUser(string id, string role)
        {
            var result = await _userService.AssignRoleToUserAsync(id, role);
            if (!result)
                return BadRequest("Falha ao atribuir papel ao usuário");
                
            return NoContent();
        }
        
        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            var roles = await _userService.GetUserRolesAsync(id);
            return Ok(roles);
        }
    }
}
