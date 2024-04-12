## **Passo a Passo de Mapeamento de Objeto para Requisição de Login**

**Objetivo:** Criar um mapeamento de objeto para a requisição de login em ASP.NET Core 8.0.

**Modelo:**

```csharp
public class Usuario
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Email { get; set; }

    [Required]
    [StringLength(50)]
    public string Senha { get; set; }
}
```

**Controller:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;

    public AuthController(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] UsuarioLoginModel model)
    {
        // Validar o modelo
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Buscar o usuário no repositório
        var usuario = await _usuarioRepository.GetByEmailAsync(model.Email);

        // Verificar se o usuário existe
        if (usuario == null)
        {
            return NotFound();
        }

        // Validar a senha
        if (!BCrypt.Net.BCrypt.Verify(model.Senha, usuario.Senha))
        {
            return Unauthorized();
        }

        // Gerar o token de autenticação
        var token = GenerateToken(usuario);

        // Retornar o token
        return Ok(new { token });
    }
}
```

**Model de Requisição:**

```csharp
public class UsuarioLoginModel
{
    [Required]
    [StringLength(50)]
    public string Email { get; set; }

    [Required]
    [StringLength(50)]
    public string Senha { get; set; }
}
```

**Mapeamento de Objeto:**

1. **Criar um model de requisição:** `UsuarioLoginModel` com as propriedades `Email` e `Senha`.
2. **Na ação de login:**
    * Receber o modelo de requisição como parâmetro `[FromBody] UsuarioLoginModel model`.
    * Validar o modelo usando `ModelState.IsValid`.
3. **Buscar o usuário no repositório:** `var usuario = await _usuarioRepository.GetByEmailAsync(model.Email)`.
4. **Verificar se o usuário existe:** Se não existir, retornar `NotFound`.
5. **Validar a senha:** Comparar a senha digitada com a senha do usuário no banco de dados usando `BCrypt.Net.BCrypt.Verify`.
6. **Gerar o token de autenticação:** Chamar um método que gera o token (`GenerateToken`).
7. **Retornar o token:** Retornar um objeto JSON com o token (`Ok(new { token })`).

**Observações:**

* Este é um exemplo simplificado de mapeamento de objeto para requisição de login.
* Em aplicações reais, você pode ter outras validações e regras de negócio.
* É importante usar criptografia para armazenar as senhas dos usuários no banco de dados.
* Você pode usar bibliotecas como IdentityServer4 para implementar autenticação e autorização em seu aplicativo.

**Recursos Adicionais:**

* Documentação oficial do ASP.NET Core sobre Models: [URL inválido removido]
* Tutoriais sobre mapeamento de objetos:
    * [https://www.youtube.com/watch?v=3LgG4aZPxJI](https://www.youtube.com/watch?v=3LgG4aZPxJI)
    * [https://www.youtube.com/watch?v=n1JrknztoY0](https://www.youtube.com/watch?v=n1JrknztoY0)

**Espero que este passo a passo detalhado seja útil para você!**
