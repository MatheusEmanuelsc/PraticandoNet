

## Índice

1. [Introdução](#introdução)
2. [Criação do Projeto de Teste](#criação-do-projeto-de-teste)
3. [Configuração do Ambiente de Teste](#configuração-do-ambiente-de-teste)
4. [Criação dos Mocks](#criação-dos-mocks)
5. [Exemplos de Testes com Mock](#exemplos-de-testes-com-mock)
   - [Testes para Métodos GET](#testes-para-métodos-get)
   - [Testes para Métodos POST](#testes-para-métodos-post)
   - [Testes para Métodos PUT](#testes-para-métodos-put)
   - [Testes para Métodos DELETE](#testes-para-métodos-delete)

## Introdução

Os testes unitários com mocks são úteis para simular o comportamento de dependências externas, como repositórios e serviços, permitindo que você foque na lógica do controlador. `Moq` é uma biblioteca popular para criar mocks em .NET.

## Criação do Projeto de Teste

1. **Criar o Projeto de Teste**

   No Visual Studio, crie um novo projeto de **XUnit Test Project**. Certifique-se de que o projeto de teste esteja no mesmo solution que o projeto da Web API.

2. **Adicionar Referência ao Projeto Principal**

   Adicione uma referência ao projeto da Web API no projeto de teste. Clique com o botão direito no projeto de teste, selecione **Adicionar** > **Referência de Projeto** e escolha o projeto principal.

3. **Adicionar Pacotes Necessários**

   Abra o **Gerenciador de Pacotes NuGet** e instale os seguintes pacotes:
   - `Moq`
   - `FluentAssertions`
   - `Xunit`

## Configuração do Ambiente de Teste

Crie uma classe base para configurar o ambiente de teste e inicializar os mocks. Esta classe vai definir o comportamento esperado dos mocks e criar instâncias do controlador com os mocks injetados.

```csharp
using Moq;
using APICatalogo.Controllers;
using APICatalogo.Repositories;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace ApiCatalogoxUnitTests.UnitTests
{
    public class ProdutosUnitTestController
    {
        public Mock<IUnitOfWork> MockRepository { get; private set; }
        public IMapper Mapper { get; private set; }
        public ProdutosController Controller { get; private set; }

        public ProdutosUnitTestController()
        {
            // Configuração do Mapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProdutoDTOMappingProfile());
            });
            Mapper = config.CreateMapper();

            // Criação do Mock para o repositório
            MockRepository = new Mock<IUnitOfWork>();

            // Criação do controlador com o mock
            Controller = new ProdutosController(MockRepository.Object, Mapper);
        }
    }
}
```

## Criação dos Mocks

Usaremos `Moq` para criar mocks das dependências e definir o comportamento esperado para nossos testes.

### Exemplo de Mock para Método GET

Vamos criar um teste para o método `GET` do controlador `ProdutosController`. Neste exemplo, mockamos o método do repositório para retornar um produto específico.

```csharp
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ApiCatalogoxUnitTests.UnitTests
{
    public class GetProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
    {
        private readonly ProdutosController _controller;
        private readonly Mock<IUnitOfWork> _mockRepository;

        public GetProdutoUnitTests(ProdutosUnitTestController fixture)
        {
            _controller = fixture.Controller;
            _mockRepository = fixture.MockRepository;
        }

        [Fact]
        public async Task GetProdutoById_Returns_OkResult_With_ProdutoDTO()
        {
            // Arrange
            var produtoId = 1;
            var produtoDto = new ProdutoDTO
            {
                ProdutoId = produtoId,
                Nome = "Produto Teste",
                Descricao = "Descrição do Produto Teste"
            };
            
            _mockRepository.Setup(repo => repo.Produtos.GetByIdAsync(produtoId))
                .ReturnsAsync(produtoDto);

            // Act
            var result = await _controller.Get(produtoId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeOfType<ProdutoDTO>();
        }

        [Fact]
        public async Task GetProdutoById_Returns_NotFound()
        {
            // Arrange
            var produtoId = 999;
            _mockRepository.Setup(repo => repo.Produtos.GetByIdAsync(produtoId))
                .ReturnsAsync((ProdutoDTO)null);

            // Act
            var result = await _controller.Get(produtoId);

            // Assert
            var notFoundResult = result.Result as NotFoundResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }
    }
}
```

### Exemplo de Mock para Método POST

No exemplo a seguir, mockamos a inserção de um novo produto e verificamos se o controlador retorna o status de `Created`.

```csharp
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ApiCatalogoxUnitTests.UnitTests
{
    public class PostProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
    {
        private readonly ProdutosController _controller;
        private readonly Mock<IUnitOfWork> _mockRepository;

        public PostProdutoUnitTests(ProdutosUnitTestController fixture)
        {
            _controller = fixture.Controller;
            _mockRepository = fixture.MockRepository;
        }

        [Fact]
        public async Task PostProduto_Returns_CreatedStatusCode()
        {
            // Arrange
            var novoProdutoDto = new ProdutoDTO
            {
                Nome = "Novo Produto",
                Descricao = "Descrição do Novo Produto"
            };

            _mockRepository.Setup(repo => repo.Produtos.AddAsync(It.IsAny<ProdutoDTO>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(novoProdutoDto);

            // Assert
            var createdResult = result.Result as CreatedAtRouteResult;
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task PostProduto_Returns_BadRequest()
        {
            // Arrange
            ProdutoDTO produtoDto = null;

            // Act
            var result = await _controller.Post(produtoDto);

            // Assert
            var badRequestResult = result.Result as BadRequestResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }
    }
}
```

### Exemplo de Mock para Método PUT

Mockamos a atualização de um produto e verificamos se o controlador retorna o status de `Ok`.

```csharp
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ApiCatalogoxUnitTests.UnitTests
{
    public class PutProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
    {
        private readonly ProdutosController _controller;
        private readonly Mock<IUnitOfWork> _mockRepository;

        public PutProdutoUnitTests(ProdutosUnitTestController fixture)
        {
            _controller = fixture.Controller;
            _mockRepository = fixture.MockRepository;
        }

        [Fact]
        public async Task PutProduto_Returns_OkResult()
        {
            // Arrange
            var produtoId = 1;
            var produtoDto = new ProdutoDTO
            {
                ProdutoId = produtoId,
                Nome = "Produto Atualizado",
                Descricao = "Descrição Atualizada"
            };

            _mockRepository.Setup(repo => repo.Produtos.UpdateAsync(produtoId, produtoDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Put(produtoId, produtoDto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task PutProduto_Returns_BadRequest()
        {
            // Arrange
            var produtoId = 1;
            ProdutoDTO produtoDto = null;

            // Act
            var result = await _controller.Put(produtoId, produtoDto);

            // Assert
            var badRequestResult = result.Result as BadRequestResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }
    }
}
```

### Exemplo de Mock para Método DELETE

Mockamos a exclusão de um produto e verificamos se o controlador retorna o status de `Ok`.

```csharp
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ApiCatalogoxUnitTests.UnitTests
{
    public class DeleteProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
   

 {
        private readonly ProdutosController _controller;
        private readonly Mock<IUnitOfWork> _mockRepository;

        public DeleteProdutoUnitTests(ProdutosUnitTestController fixture)
        {
            _controller = fixture.Controller;
            _mockRepository = fixture.MockRepository;
        }

        [Fact]
        public async Task DeleteProduto_Returns_OkResult()
        {
            // Arrange
            var produtoId = 1;

            _mockRepository.Setup(repo => repo.Produtos.DeleteAsync(produtoId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(produtoId);

            // Assert
            var okResult = result as OkResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task DeleteProduto_Returns_NotFound()
        {
            // Arrange
            var produtoId = 999;
            _mockRepository.Setup(repo => repo.Produtos.DeleteAsync(produtoId))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Delete(produtoId);

            // Assert
            var notFoundResult = result as NotFoundResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }
    }
}
```

## Conclusão

O uso de mocks permite que você isole o controlador da API, simulando o comportamento de suas dependências. Isso facilita a verificação do comportamento da lógica de controle sem depender de uma implementação real de repositórios ou serviços. Usando `Moq`, você pode criar e configurar mocks para definir comportamentos esperados e testar diferentes cenários de forma eficaz.