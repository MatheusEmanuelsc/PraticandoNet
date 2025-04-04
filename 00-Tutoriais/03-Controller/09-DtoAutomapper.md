

### Diferença entre as Abordagens

1. **`AddAutoMapper(typeof(MappingProfile))`**:
   - Registra o AutoMapper e escaneia o assembly que contém o tipo especificado (neste caso, `MappingProfile`) para encontrar todos os perfis de mapeamento automaticamente.
   - **Vantagem**: Simples e útil quando você tem vários perfis no mesmo assembly e quer que todos sejam carregados sem listá-los explicitamente.
   - **Desvantagem**: Menos controle explícito; pode carregar perfis indesejados se houver outros no assembly.

2. **`AddAutoMapper(config => config.AddProfile<MappingProfile>())`**:
   - Permite configurar o AutoMapper explicitamente, adicionando apenas os perfis que você especifica no delegate.
   - **Vantagem**: Mais controle sobre quais perfis são registrados, ideal para projetos onde você quer evitar carregamentos automáticos.
   - **Desvantagem**: Requer mais código se você tiver muitos perfis para adicionar manualmente.

Eu usei a segunda abordagem no tutorial anterior por preferir explicitar os perfis em exemplos simples, mas ambas são igualmente válidas dependendo do contexto. Vou refazer o resumo incluindo as duas formas.

---

# DTOs e AutoMapper no ASP.NET Core (Revisado)

## Índice
1. [O que são DTOs?](#o-que-são-dtos)
2. [O que é AutoMapper?](#o-que-é-automapper)
3. [Por que Usar DTOs e AutoMapper?](#por-que-usar-dtos-e-automapper)
4. [Tutorial Passo a Passo](#tutorial-passo-a-passo)
   - [Passo 1: Instalar o AutoMapper](#passo-1-instalar-o-automapper)
   - [Passo 2: Definir o Modelo e DTOs](#passo-2-definir-o-modelo-e-dtos)
   - [Passo 3: Configurar o AutoMapper](#passo-3-configurar-o-automapper)
   - [Passo 4: Configurar no Program.cs (Duas Opções)](#passo-4-configurar-no-programcs-duas-opções)
   - [Passo 5: Usar no Controller](#passo-5-usar-no-controller)
   - [Passo 6: Testar os Endpoints](#passo-6-testar-os-endpoints)
5. [Boas Práticas](#boas-práticas)
6. [Conclusão](#conclusão)

---

## O que são DTOs?

**DTOs (Data Transfer Objects)** são objetos simples usados para transferir dados entre camadas, expondo apenas os dados necessários.

---

## O que é AutoMapper?

**AutoMapper** é uma biblioteca que automatiza o mapeamento entre objetos (ex.: modelo para DTO), baseando-se em convenções ou configurações personalizadas.

---

## Por que Usar DTOs e AutoMapper?

- **DTOs**: Controlam a exposição de dados e adaptam o formato para o cliente.
- **AutoMapper**: Reduz código boilerplate e erros em conversões.

---

## Tutorial Passo a Passo

### Passo 1: Instalar o AutoMapper

Adicione o pacote:
```
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

### Passo 2: Definir o Modelo e DTOs

```csharp
namespace MeuProjeto.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
}

namespace MeuProjeto.Dtos;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
}

public class UsuarioCreateDto
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
}
```

### Passo 3: Configurar o AutoMapper

Crie o perfil de mapeamento.

```csharp
using AutoMapper;
using MeuProjeto.Models;
using MeuProjeto.Dtos;

namespace MeuProjeto.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioDto>();
        CreateMap<UsuarioCreateDto, Usuario>();
    }
}
```

### Passo 4: Configurar no Program.cs (Duas Opções)

Registre o AutoMapper com uma das duas abordagens:

```csharp
using MeuProjeto.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Opção 1: Usando typeof (escaneia o assembly)
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Opção 2: Usando delegate (explícito)
// builder.Services.AddAutoMapper(config => config.AddProfile<MappingProfile>());

var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();
```

- **Opção 1**: Use `typeof` se tiver vários perfis no mesmo assembly e quiser carregá-los automaticamente.
- **Opção 2**: Use o delegate para controle explícito sobre os perfis registrados.

### Passo 5: Usar no Controller

```csharp
using AutoMapper;
using MeuProjeto.Dtos;
using MeuProjeto.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeuProjeto.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly List<Usuario> _usuarios = new()
    {
        new() { Id = 1, Nome = "João", Email = "joao@email.com", Senha = "123" }
    };

    public UsuariosController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        await Task.Delay(50);
        var usuariosDto = _mapper.Map<IEnumerable<UsuarioDto>>(_usuarios);
        return Ok(usuariosDto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        await Task.Delay(50);
        var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario == null) return NotFound();
        var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
        return Ok(usuarioDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] UsuarioCreateDto usuarioDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var usuario = _mapper.Map<Usuario>(usuarioDto);
        usuario.Id = _usuarios.Max(u => u.Id) + 1;
        await Task.Delay(50);
        _usuarios.Add(usuario);
        var resultadoDto = _mapper.Map<UsuarioDto>(usuario);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = usuario.Id }, resultadoDto);
    }
}
```

### Passo 6: Testar os Endpoints

- **GET /api/usuarios**:
  - Resposta: `200 OK` com `[{"id": 1, "nome": "João", "email": "joao@email.com"}]`
- **GET /api/usuarios/1**:
  - Resposta: `200 OK` com `{"id": 1, "nome": "João", "email": "joao@email.com"}`
- **POST /api/usuarios** com `{ "nome": "Maria", "email": "maria@email.com", "senha": "456" }`:
  - Resposta: `201 Created` com `{"id": 2, "nome": "Maria", "email": "maria@email.com"}`

---

## Boas Práticas

1. **DTOs específicos**: Crie DTOs distintos para entrada e saída.
2. **Mapeamentos explícitos**: Prefira o delegate (`AddProfile`) para controle em projetos pequenos; use `typeof` para múltiplos perfis.
3. **Evite overmapping**: Não mapeie campos desnecessários ou sensíveis.
4. **Validação**: Valide DTOs antes de mapear para o modelo.
5. **Teste os mapeamentos**: Certifique-se de que o AutoMapper reflete as regras corretamente.

---

## Conclusão

DTOs controlam a exposição de dados, enquanto o AutoMapper simplifica o mapeamento entre modelos e DTOs. Este tutorial revisado mostra como usar ambas as formas de registro (`typeof` e delegate), oferecendo flexibilidade para diferentes cenários. A escolha entre elas depende do tamanho do projeto e da necessidade de controle explícito. É uma combinação essencial para APIs limpas e eficientes.
