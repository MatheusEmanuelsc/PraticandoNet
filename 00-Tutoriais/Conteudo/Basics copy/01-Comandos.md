### .NET 8 CLI - Comandos Essenciais

#### 1. Criar uma Web API

- **Comando básico:**
  ```bash
  dotnet new webapi
  ```
  Cria um novo projeto de Web API com a estrutura padrão.
  
- **Com controladores pré-definidos:**
  ```bash
  dotnet new webapi -controllers
  ```
  Cria uma Web API com controladores já configurados.

#### 2. Gerenciar Pacotes NuGet

- **Adicionar pacotes:**
  ```bash
  dotnet add package <NOME_DO_PACOTE>
  ```
  Instala um pacote NuGet no projeto.

- **Exemplo (AutoMapper):**
  ```bash
  dotnet add package AutoMapper
  ```
  Adiciona o AutoMapper para mapeamento de objetos.

#### 3. Trabalhar com Entity Framework Core (EF Core)

- **Pacotes necessários:**
  - `Microsoft.EntityFrameworkCore`: Pacote principal do EF Core.
  - `Microsoft.EntityFrameworkCore.Tools`: Ferramentas de linha de comando para EF Core.

- **Provedores de banco de dados:**
  - **MySQL:**
    ```bash
    dotnet add package Pomelo.EntityFrameworkCore.MySql
    ```
    Adiciona o provedor MySQL.
  
  - **SQLite:**
    ```bash
    dotnet add package Microsoft.EntityFrameworkCore.Sqlite
    ```
    Adiciona o provedor SQLite.

#### 4. Ferramentas EF Core

- **Instalar ferramentas do EF Core globalmente:**
  ```bash
  dotnet tool install --global dotnet-ef
  ```
  
- **Atualizar ferramentas do EF Core:**
  ```bash
  dotnet tool update --global dotnet-ef
  ```

- **Verificar comandos disponíveis:**
  ```bash
  dotnet ef
  ```
  Exibe todos os comandos disponíveis do EF Core.

#### 5. Executar Migrações

- **Criar uma nova migração:**
  ```bash
  dotnet ef migrations add <NOME_DA_MIGRACAO>
  ```
  Cria uma migração com as alterações no modelo de dados.

- **Atualizar o banco de dados com as migrações:**
  ```bash
  dotnet ef database update
  ```

#### 6. Drivers de Banco de Dados

- **Provedor MySQL:**
  ```bash
  Pomelo.EntityFrameworkCore.MySql
  ```
  Pacote para usar MySQL com EF Core.

- **Provedor SQLite:**
  ```bash
  Microsoft.EntityFrameworkCore.Sqlite
  ```

---

### Exemplo de Readme.md

```markdown
## Criando uma Web API com .NET 8

1. Crie um novo projeto de Web API:
    ```bash
    dotnet new webapi
    ```

2. Instale o pacote AutoMapper:
    ```bash
    dotnet add package AutoMapper
    ```

3. Adicione pacotes do Entity Framework Core e o provedor MySQL:
    ```bash
    dotnet add package Microsoft.EntityFrameworkCore
    dotnet add package Microsoft.EntityFrameworkCore.Tools
    dotnet add package Pomelo.EntityFrameworkCore.MySql
    ```

4. Crie uma nova migração:
    ```bash
    dotnet ef migrations add MinhaPrimeiraMigracao
    ```

5. Aplique a migração ao banco de dados:
    ```bash
    dotnet ef database update
    ```

6. Inicie a API:
    ```bash
    dotnet run
    ```



---

### Observações Finais

Este guia apresenta os principais comandos para começar a desenvolver uma Web API com .NET 8 e gerenciar o EF Core com diferentes provedores de banco de dados. Para mais detalhes, consulte a documentação oficial do .NET CLI e EF Core.