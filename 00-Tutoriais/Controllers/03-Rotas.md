## Roteamento, Padrões e Restrições de Rotas no ASP.NET Core: Um Guia Completo

**Introdução:**

O roteamento é essencial no ASP.NET Core, mapeando URLs para ações do controlador. Este guia abrange roteamento convencional, baseado em atributos, padrões de rotas e restrições detalhadas, incluindo valores mínimos, máximos, intervalos e expressões regulares.

**Roteamento Convencional:**

O roteamento convencional utiliza modelos predefinidos para mapear URLs comuns (`Index`, `Details`, etc.).

```c#
routes.MapRoute(
    name: "default",
    template: "{controller=Home}/{action=Index}/{id?}"
);
```

**Roteamento Baseado em Atributos:**

Oferece mais controle com atributos em controladores e ações:

```c#
[Route("products/{id}")]
public class ProductsController : Controller
{
    [HttpGet]
    public IActionResult Details(int id) { /* ... */ }
}
```

**Padrões de Rotas:**

Definem a estrutura da URL com segmentos literais e variáveis:

* `{controller}`: Nome do controlador (ex: `Home`, `Products`).
* `{action}`: Nome da ação do controlador (ex: `Index`, `Details`).
* `{id?}`: Parâmetro opcional `id` (número inteiro).

**Restrições de Rotas:**

Limitam quais URLs podem corresponder a uma rota, validando e filtrando:

**Tipos de Restrições:**

* **Restrição de Valor:** Limita o tipo de valor (int, string, data).
    * `int:min:18,max:100`: Idade entre 18 e 100.
    * `decimal:range:10.00,20.00`: Preço entre 10.00 e 20.00.
    * `datetime:min:2023-01-01,max:2024-12-31`: Data entre 2023-01-01 e 2024-12-31.
    * `string:length:5,max:100`: Nome entre 5 e 100 caracteres.
    * `bool`: `true` ou `false`.
    * `guid`: GUID.
* **Restrição Regular:** Valida o formato com expressões regulares.
    * `int:regex:^([0-9]{3})-([0-9]{3})$`: Telefone no formato XXX-XXX.
* **Restrição Personalizada:** Implementa lógica personalizada para validação.

**Exemplo:**

```c#
[Route("products/{id:int:min:10,max:100}")]
public class ProductsController : Controller
{
    [HttpGet]
    public IActionResult Details(int id) { /* ... */ }
}
```

**Conclusão:**

As restrições de valor no ASP.NET Core fornecem uma ferramenta poderosa para garantir que apenas URLs válidas sejam processadas por suas ações do controlador. Ao utilizar os diversos tipos de restrições disponíveis, você pode validar valores, definir limites, aplicar formatos e até mesmo implementar validações personalizadas, tornando seu código mais robusto e seguro.
