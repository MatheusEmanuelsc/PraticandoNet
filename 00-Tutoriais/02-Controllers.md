## **Resumo Completo de Controladores C# no ASP.NET Core 8.0**

**Introdução**

Um controlador é uma classe fundamental no ASP.NET Core que manipula solicitações HTTP e gera respostas. Ele atua como o ponto central do roteamento de URLs para ações específicas em seu aplicativo MVC.

**Criando um Controlador**

**1. Usando o Scaffolding do Visual Studio:**

* Clique com o botão direito na pasta **Controladores** e selecione **Adicionar** > **Novo Item**.
* Escolha o modelo **Controlador MVC - Vazio**.
* Insira o nome do controlador (por exemplo, "HelloWorldController") e clique em **Adicionar**.

**2. Criando manualmente a classe do controlador:**

* Crie uma nova classe C# na pasta **Controladores**.
* Nomeie a classe com o sufixo **Controller** (por exemplo, "HelloWorldController").
* Faça a classe herdar da classe base `Controller` do ASP.NET Core.

**Exemplo de código:**

```csharp
public class HelloWorldController : Controller
{
    // Ação para a rota GET: /HelloWorld
    public IActionResult Index()
    {
        // Retorna a view "Index"
        return View();
    }

    // Ação para a rota GET: /HelloWorld/Welcome
    public IActionResult Welcome(string name)
    {
        // Retorna a view "Welcome" com a variável "name"
        return View(name);
    }
}
```

**Roteamento de URLs**

O ASP.NET Core usa um sistema de roteamento para mapear URLs para ações em seus controladores. As rotas são definidas usando atributos na classe do controlador:

* **[HttpGet]:** Define uma ação para a rota GET.
* **[HttpPost]:** Define uma ação para a rota POST.
* **[Route]:** Especifica a rota URL para a ação.

**Exemplo de código:**

```csharp
[HttpGet]
[Route("/HelloWorld/Details/{id}")]
public IActionResult Details(int id)
{
    // Retorna a view "Details" com o ID especificado
    return View(id);
}
```

**Mapeamento de Rotas**

O mapeamento de rotas configura como as URLs são mapeadas para ações em seus controladores. O ASP.NET Core oferece um sistema de roteamento flexível que permite mapear URLs de várias maneiras.

**Exemplo de código:**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllersWithViews();

    services.AddRouting(options =>
    {
        options.AppendTrailingSlash = true;
        options.LowercaseUrls = true;

        options.MapControllerRoute(
            name: "default",
            pattern: "{controller}/{action}/{id?}",
            defaults: new { controller = "Home", action = "Index" }
        );
    });
}
```

<!-- **Views**

As views são arquivos HTML que contêm o código Razor para gerar a resposta final para o cliente. As views são associadas às ações do controlador usando o método `View()`.

**Exemplo de código:**

```html
@model string

<h1>Bem-vindo, @Model!</h1>
```

**ViewData e ViewBag**

ViewData e ViewBag são usados para passar dados da ação para a view.

* **ViewData:** Um dicionário dinâmico que pode armazenar qualquer tipo de objeto.
* **ViewBag:** Uma classe dinâmica que fornece propriedades para armazenar valores.

**Exemplo de código:**

```csharp
public IActionResult Index()
{
    ViewData["Message"] = "Olá, mundo!";
    ViewBag.Name = "John Doe";

    return View();
}
```

**Exemplo de código (view):**

```html
@ViewData["Message"]

<p>Nome: @ViewBag.Name</p>
``` -->

**Data Annotations**

Data Annotations são atributos que podem ser aplicados a propriedades de classes para fornecer metadados adicionais sobre a propriedade. Esses metadados podem ser usados para validação de dados, formatação e outras funcionalidades.

**Exemplos de Data Annotations:**

* **[Required]:** Indica que a propriedade é obrigatória e não pode ser nula.
* **[StringLength]:** Define o tamanho máximo da string que pode ser armazenada na propriedade.
* **[Range]:** Define um intervalo válido de valores para a propriedade.

**Exemplo de código:**

```csharp
public class Person
{
    [Required]
    public string Name { get; set; }

    [StringLength(50)]
    public string Address { get; set; }

    [Range(18, 120)]
