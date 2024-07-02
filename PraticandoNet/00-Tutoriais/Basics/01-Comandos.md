## .NET 8 CLI - Comandos para Readme.md

### Criar Web API

* `dotnet new webapi` - Cria um projeto de Web API vazio.
* `dotnet new webapi -controllers` - Cria um projeto de Web API com controladores pré-definidos.

### Gerenciar Pacotes

* `dotnet add package NOME_PACOTE` - Instala um pacote NuGet.
* `dotnet add package AutoMap` - Instala o AutoMapper para mapeamento de objetos.

### Trabalhar com Entity Framework Core (EF Core)

* **Pacotes:**
    * `Microsoft.EntityFrameworkCore` - Pacote principal do EF Core.
    * `Microsoft.EntityFrameworkCore.tools` - Ferramentas de linha de comando do EF Core.
* **MySQL:**
    * `dotnet add package Pomelo.EntityFrameworkCore.MySql` - Provedor MySQL para EF Core.
* **SQLite:**
    * `dotnet add package Microsoft.EntityFrameworkCore.Sqlite` - Provedor SQLite para EF Core.

### Ferramentas EF Core

* `dotnet tool install --global dotnet-ef` - Instala as ferramentas EF Core globalmente.
* `dotnet tool update --global dotnet-ef` - Atualiza as ferramentas EF Core.
* `dotnet ef` - Exibe comandos do EF Core.

### Executar Migrações

* `dotnet ef migrations add NOME_MIGRACAO` - Cria uma nova migração.
* `dotnet ef database update` - Aplica as migrações pendentes ao banco de dados.

### Drivers de Banco de Dados

* **MySQL:**
    * `Pomelo.EntityFrameworkCore.MySql` - Provedor MySQL para EF Core.
* **SQLite:**
    * `Microsoft.EntityFrameworkCore.Sqlite` - Provedor SQLite para EF Core.

**Observações:**

* Este guia é um resumo dos comandos mais comuns. Consulte a documentação oficial do .NET CLI e do EF Core para obter mais informações.
* O comando `dotnet ef` possui diversos subcomandos para trabalhar com migrações, modelos e bancos de dados.

**Exemplo de Readme.md:**

```markdown
## Criando uma Web API com .NET 8

1. Crie um novo projeto de Web API:

```
dotnet new webapi
```

2. Instale o pacote AutoMapper:

```
dotnet add package AutoMap
```

3. Adicione referências ao EF Core e ao provedor Pomelo para MySQL:

```
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.tools
Pomelo.EntityFrameworkCore.MySql
```

4. Crie uma nova migração:

```
dotnet ef migrations add MinhaPrimeiraMigracao
```

5. Aplique a migração ao banco de dados MySQL:

```
dotnet ef database update
```

6. Inicie a API:

```
dotnet run
```

## Recursos Adicionais

* [Documentação do .NET CLI](https://dotnet.microsoft.com/platform/dotnet-cli)
* [Documentação do EF Core](https://docs.microsoft.com/ef/)
```
