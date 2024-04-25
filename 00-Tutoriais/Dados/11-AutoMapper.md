## Mapeando objetos com AutoMapper no ASP.NET Core (.md)

**Introdução**

O AutoMapper é uma biblioteca poderosa para mapear objetos em .NET. Ele simplifica a conversão entre diferentes estruturas de dados, tornando seu código mais limpo e fácil de manter. Neste guia, vamos ver como usar o AutoMapper em uma aplicação ASP.NET Core para mapear entre DTOs (Data Transfer Objects) e entidades de domínio.

**Passo a Passo**

**1. Instalar Pacotes**

Comece instalando os pacotes AutoMapper e AutoMapper.Extensions.Microsoft.DependencyInjection no seu projeto:

```bash
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

**2. Configurar o AutoMapper**

Na classe `Startup` do seu projeto, adicione o seguinte código para configurar o AutoMapper:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ... outros serviços ...

    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
}
```

Este código irá automaticamente descobrir e registrar todos os perfis de mapeamento que você criar em seu projeto.

**3. Criar Perfis de Mapeamento**

Crie um perfil de mapeamento para cada tipo de objeto que você deseja mapear. Por exemplo, se você deseja mapear entre usuários e DTOs de usuário, crie um perfil como este:

```csharp
using AutoMapper;
using _03_DtoAutoMapper.Domain.Models; // Importar modelos de domínio

namespace _03_DtoAutoMapper.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResponseDto>();
            CreateMap<UserRequestDto, User>();
        }
    }
}
```

Neste perfil, você define como as propriedades entre os objetos `User` e `UserResponseDto` (ou `UserRequestDto` e `User`) devem ser mapeadas.

**4. Injetar o AutoMapper**

Em seus controladores, injete a interface `IMapper` do AutoMapper no construtor:

```csharp
using AutoMapper;

public class UserController : Controller
{
    private readonly IMapper _mapper;

    public UserController(IMapper mapper)
    {
        _mapper = mapper;
    }

    // ... métodos do controlador ...
}
```

**5. Mapear Objetos**

Para mapear objetos, utilize o método `Map` da instância `IMapper`:

```csharp
// Mapear um User para um UserResponseDto
User user = new User();
UserResponseDto userDto = _mapper.Map<UserResponseDto>(user);

// Mapear um UserRequestDto para um User
UserRequestDto userRequest = new UserRequestDto();
User newUser = _mapper.Map<User>(userRequest);
```

**Exemplo Completo**

Aqui está um exemplo completo de como mapear objetos com AutoMapper em um controlador ASP.NET Core:

```csharp
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using _03_DtoAutoMapper.Domain.Models;
using _03_DtoAutoMapper.Domain.Profiles;

namespace _03_DtoAutoMapper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IMapper _mapper;

        public UserController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserRequestDto userRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Mapear o UserRequestDto para um User
            var user = _mapper.Map<User>(userRequest);

            // Salvar o usuário no banco de dados
            // ...

            // Mapear o User salvo para um UserResponseDto
            var userDto = _mapper.Map<UserResponseDto>(user);

            return CreatedAtAction("GetUser", new { id = user.Id }, userDto);
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            // Obter o usuário do banco de dados por ID
            // ...

            // Mapear o User para um UserResponseDto
            var userDto = _mapper.Map<UserResponseDto>(user);

            return Ok(userDto);
        }
    }
}
```

**Observações**

* Você pode usar o AutoMapper para mapear entre qualquer tipo de objeto, não apenas DTOs e entidades de domínio.
* O AutoMapper oferece diversas opções de configuração para personalizar o mapeamento de objetos.
* Para mais informações sobre o AutoMapper,