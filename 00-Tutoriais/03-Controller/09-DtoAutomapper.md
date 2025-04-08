

```md
# DTOs e AutoMapper no ASP.NET Core 8 (Atualizado)

## 📌 Índice
1. [O que são DTOs?](#o-que-são-dtos)
2. [O que é AutoMapper?](#o-que-é-automapper)
3. [Por que Usar DTOs e AutoMapper?](#por-que-usar-dtos-e-automapper)
4. [Instalação do AutoMapper](#instalação-do-automapper)
5. [Exemplo de Modelo e DTOs](#exemplo-de-modelo-e-dtos)
6. [Criando o MappingProfile](#criando-o-mappingprofile)
7. [Configurando no Program.cs](#configurando-no-programcs)
   - [Opção 1: typeof() (escaneamento automático)](#opção-1-usando-typeof---escaneamento-automático)
   - [Opção 2: Delegate AddProfile (explícito)](#opção-2-usando-delegate-com-addprofile---explícito)
   - [Opção 3: Registrar múltiplos perfis manualmente](#opção-3-registrar-múltiplos-perfis-manualmente)
8. [Usando o AutoMapper no Controller](#usando-o-automapper-no-controller)
9. [Boas Práticas](#boas-práticas)
10. [Conclusão](#conclusão)

---

## O que são DTOs?

**DTOs (Data Transfer Objects)** são objetos usados para transportar dados entre processos e camadas da aplicação. Eles servem para:
- Expor somente os dados necessários para o cliente.
- Separar as regras da entidade de domínio da representação da API.

---

## O que é AutoMapper?

**AutoMapper** é uma biblioteca que permite mapear automaticamente objetos de um tipo para outro (ex: `Usuario` para `UsuarioDto`), reduzindo código repetitivo e aumentando a coesão.

---

## Por que Usar DTOs e AutoMapper?

- Separação de responsabilidades.
- Prevenção de exposição de dados sensíveis.
- Redução de código de mapeamento manual.
- Padronização e consistência na transformação de dados.

---

## Instalação do AutoMapper

A partir da versão **13**, o AutoMapper já inclui suporte à injeção de dependência, e **não é mais necessário instalar o pacote `AutoMapper.Extensions.Microsoft.DependencyInjection`**.

```bash
dotnet add package AutoMapper
```

---

## Exemplo de Modelo e DTOs

```csharp
// Models/Usuario.cs
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; } // Campo interno, não deve ir pro DTO
}
```

```csharp
// Dtos/UsuarioDto.cs
public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
}

// Dtos/UsuarioCreateDto.cs
public class UsuarioCreateDto
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
}
```

---

## Criando o MappingProfile

```csharp
// Mappings/MappingProfile.cs
using AutoMapper;
using MeuProjeto.Models;
using MeuProjeto.Dtos;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioDto>();
        CreateMap<UsuarioCreateDto, Usuario>();
    }
}
```

---

## Configurando no Program.cs

### Opção 1: Usando `typeof` – escaneamento automático

```csharp
builder.Services.AddAutoMapper(typeof(MappingProfile));
```

- Busca todos os perfis no assembly onde está o `MappingProfile`.
- Boa para projetos médios/grandes com muitos perfis organizados no mesmo assembly.

---

### Opção 2: Usando delegate com `AddProfile` – explícito

```csharp
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});
```

- Adiciona perfis individualmente.
- Maior controle e previsibilidade (ótimo para testes, microserviços ou perfis em assemblies diferentes).

---

### Opção 3: Registrar múltiplos perfis manualmente

```csharp
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile(new MappingProfile());
    cfg.AddProfile(new OutroPerfil());
});
```

- Útil se você quiser configurar instâncias específicas de perfis.

---

## Usando o AutoMapper no Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IMapper _mapper;

    // Simulação de banco
    private static readonly List<Usuario> _usuarios = new()
    {
        new Usuario { Id = 1, Nome = "João", Email = "joao@email.com", Senha = "123" }
    };

    public UsuariosController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var dto = _mapper.Map<IEnumerable<UsuarioDto>>(_usuarios);
        return Ok(dto);
    }

    [HttpPost]
    public IActionResult Create([FromBody] UsuarioCreateDto input)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var usuario = _mapper.Map<Usuario>(input);
        usuario.Id = _usuarios.Max(x => x.Id) + 1;

        _usuarios.Add(usuario);

        var output = _mapper.Map<UsuarioDto>(usuario);
        return CreatedAtAction(nameof(GetAll), new { id = usuario.Id }, output);
    }
}
```

---

## Boas Práticas

- ✅ Crie **DTOs distintos** para entrada e saída.
- ✅ **Evite mapear propriedades sensíveis** (ex: senhas) para DTOs de saída.
- ✅ **Valide DTOs** com FluentValidation antes de mapear.
- ✅ **Teste seus perfis** com `MapperConfiguration.AssertConfigurationIsValid()`.
- ✅ Mantenha seus perfis organizados em uma pasta como `Mappings`.

---

## Conclusão

- DTOs e AutoMapper são ferramentas essenciais para separar lógica de domínio da apresentação.
- Com o AutoMapper 13+, o registro ficou mais simples: você só precisa do pacote principal.
- A escolha entre `typeof`, `AddProfile` ou instâncias depende do controle que você deseja ter.
- As boas práticas ajudam a manter o código limpo, seguro e escalável.
```
