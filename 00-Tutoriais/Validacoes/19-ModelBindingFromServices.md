## Tutorial Completo sobre `FromServices` no .NET 8 Web API: Injeção de Dependências com Exemplos

**Introdução**

A API Web do .NET 8 oferece o recurso `FromServices` para facilitar a injeção de dependências em controllers e ações. Com o `FromServices`, você pode acessar serviços registrados no contêiner de DI da sua aplicação diretamente em seus métodos de controller, sem a necessidade de instanciá-los manualmente. 

Este tutorial completo irá te guiar pelos principais conceitos, cenários de uso, atributos e exemplos práticos para dominar o `FromServices` e criar APIs Web mais organizadas, testáveis e escaláveis.

**Benefícios do `FromServices`**

* **Organização do Código:** O `FromServices` promove a organização do código, separando a lógica de negócios da responsabilidade de gerenciar dependências.
* **Testabilidade Aprimorada:** Ao injetar dependências através do `FromServices`, seus testes unitários tornam-se mais fáceis de escrever e manter.
* **Reusabilidade de Código:** As dependências injetadas podem ser reutilizadas em diferentes partes da sua aplicação, reduzindo duplicação de código.
* **Escalabilidade:** O `FromServices` facilita a implementação de padrões de design como Inversão de Controle (IoC) e Dependency Injection (DI), que são cruciais para aplicações escaláveis.

**Cenários de Uso do `FromServices`**

O `FromServices` pode ser utilizado em diversos cenários, como:

* **Injeção de Repositórios:** Acesse repositórios de dados para recuperar e persistir informações sem precisar instanciá-los manualmente.
* **Injeção de Serviços de Negócio:** Utilize serviços de negócio para realizar tarefas complexas sem duplicar lógica em seus controllers.
* **Injeção de Ferramentas de Log:** Acesse ferramentas de log para registrar eventos e mensagens em sua aplicação.
* **Injeção de Serviços de Autenticação e Autorização:** Utilize serviços de autenticação e autorização para verificar a identidade e as permissões dos usuários.

**Atributos do `FromServices`**

O `FromServices` oferece alguns atributos para personalizar a forma como as dependências são injetadas:

* **[FromServices]:** Indica que o parâmetro deve ser preenchido com uma dependência registrada no contêiner de DI.
* **[ServiceDescriptor]:** Permite especificar o descritor de serviço para a dependência a ser injetada.
* **[Inject]:** Sinônimo de `[FromServices]`.

**Exemplos Práticos**

* **Injetando um repositório de produtos:**

```c#
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    public ProductsController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [HttpGet]
    public IActionResult GetProducts()
    {
        var products = _productRepository.GetAll();
        return Ok(products);
    }
}
```

* **Injetando um serviço de log:**

```c#
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ILogger<OrdersController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] Order order)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating order: {0}", order.Id);

        _orderService.CreateOrder(order);

        _logger.LogInformation("Order created successfully: {0}", order.Id);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
}
```

* **Injetando um serviço de autenticação:**

```c#
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest request)
