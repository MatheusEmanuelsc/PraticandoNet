
```markdown
# Utilizando o `dotnet ef` com Entity Framework Core

## Índice
1. [O que é o `dotnet ef`](#o-que-é-o-dotnet-ef)  
2. [Pré-requisitos](#pré-requisitos)  
3. [Comandos Básicos do `dotnet ef`](#comandos-básicos-do-dotnet-ef)  
   3.1. [Gerando Migrações](#gerando-migrações)  
   3.2. [Aplicando Migrações](#aplicando-migrações)  
   3.3. [Reverter Migrações](#reverter-migrações)  
   3.4. [Scaffolding (Geração de Código)](#scaffolding-geração-de-código)  
4. [Usando `dotnet ef` em Projeto Padrão (Único Projeto)](#usando-dotnet-ef-em-projeto-padrão-único-projeto)  
   4.1. [Estrutura do Projeto](#estrutura-do-projeto)  
   4.2. [Execução dos Comandos](#execução-dos-comandos)  
5. [Usando `dotnet ef` com Múltiplos Projetos](#usando-dotnet-ef-com-múltiplos-projetos)  
   5.1. [Estrutura do Projeto](#estrutura-do-projeto-1)  
   5.2. [Configuração e Execução dos Comandos](#configuração-e-execução-dos-comandos)  

---

## 1. O que é o `dotnet ef`
O `dotnet ef` é uma ferramenta de linha de comando (CLI) do Entity Framework Core que permite gerenciar migrações, atualizar bancos de dados e gerar código a partir de bancos existentes (scaffolding). Ele é essencial para tarefas de design-time fora do Visual Studio.

---

## 2. Pré-requisitos
- **Instalação do CLI**: Instale globalmente com:
  ```
  dotnet tool install --global dotnet-ef
  ```
- **Pacotes no Projeto**:
  - `Microsoft.EntityFrameworkCore.Design` (para design-time).
  - Um provedor específico (ex.: `Microsoft.EntityFrameworkCore.SqlServer` ou `Pomelo.EntityFrameworkCore.MySql`).
- **Classe de Contexto**: Configurada com `DbContext` e registrada via `AddDbContext`.

---

## 3. Comandos Básicos do `dotnet ef`

### 3.1. Gerando Migrações
Cria uma nova migração com base nas alterações no modelo:
```
dotnet ef migrations add NOME_DA_MIGRACAO
```

### 3.2. Aplicando Migrações
Atualiza o banco de dados com as migrações pendentes:
```
dotnet ef database update
```

### 3.3. Reverter Migrações
Remove a última migração aplicada (reverte o banco):
```
dotnet ef database update NOME_DA_MIGRACAO_ANTERIOR
```
Para remover a migração do código (sem aplicar):
```
dotnet ef migrations remove
```

### 3.4. Scaffolding (Geração de Código)
Gera classes de modelo e contexto a partir de um banco existente:
```
dotnet ef dbcontext scaffold "STRING_DE_CONEXAO" PROVEDOR
```
Exemplo:
```
dotnet ef dbcontext scaffold "Server=localhost;Database=MeuBanco;User Id=sa;Password=minhasenha;" Microsoft.EntityFrameworkCore.SqlServer
```

---

## 4. Usando `dotnet ef` em Projeto Padrão (Único Projeto)

### 4.1. Estrutura do Projeto
Em um projeto único, tudo (contexto, modelos e código da aplicação) está no mesmo arquivo `.csproj`. Exemplo:
```
MeuProjeto/
├── MeuContexto.cs
├── Models/
│   ├── Cliente.cs
│   └── Pedido.cs
├── Program.cs
├── appsettings.json
└── MeuProjeto.csproj
```

### 4.2. Execução dos Comandos
1. **Navegue até o diretório do projeto**:
   ```
   cd MeuProjeto
   ```
2. **Gere uma migração**:
   ```
   dotnet ef migrations add InitialCreate
   ```
   - Os arquivos de migração são gerados em uma pasta `Migrations` no mesmo projeto.
3. **Aplique ao banco**:
   ```
   dotnet ef database update
   ```
   - O banco é atualizado com base na string de conexão em `appsettings.json`.

**Nota**: Não é necessário especificar o projeto, pois o contexto e o provedor estão no mesmo `.csproj`.

---

## 5. Usando `dotnet ef` com Múltiplos Projetos

### 5.1. Estrutura do Projeto
Em uma solução com múltiplos projetos, o contexto e os modelos podem estar em um projeto separado (ex.: camada de dados), enquanto a aplicação principal está em outro. Exemplo:
```
MinhaSolucao/
├── Data/
│   ├── MeuContexto.cs
│   ├── Models/
│   │   ├── Cliente.cs
│   │   └── Pedido.cs
│   └── Data.csproj
├── WebApp/
│   ├── Program.cs
│   ├── appsettings.json
│   └── WebApp.csproj
└── MinhaSolucao.sln
```
- `Data.csproj`: Contém o contexto e os modelos, com referências ao provedor (ex.: `Microsoft.EntityFrameworkCore.SqlServer`) e ao `Microsoft.EntityFrameworkCore.Design`.
- `WebApp.csproj`: Contém a aplicação e referencia o projeto `Data`.

### 5.2. Configuração e Execução dos Comandos
1. **Especifique os projetos nos comandos**:
   - `-p` (projeto): Projeto onde está o contexto (contém as migrações).
   - `-s` (startup): Projeto que contém a configuração (ex.: `appsettings.json` e `AddDbContext`).
2. **Navegue até o diretório da solução** (opcional, mas recomendado):
   ```
   cd MinhaSolucao
   ```
3. **Gere uma migração**:
   ```
   dotnet ef migrations add InitialCreate -p Data -s WebApp
   ```
   - As migrações são geradas no projeto `Data`.
4. **Aplique ao banco**:
   ```
   dotnet ef database update -p Data -s WebApp
   ```
   - O `WebApp` fornece a string de conexão e a configuração do `DbContext`.

**Nota**: Certifique-se de que o projeto `Data` referencia o provedor e o `Microsoft.EntityFrameworkCore.Design`, e que o `WebApp` registra o `DbContext` corretamente em `Program.cs`.

---

### Diferenças Chave
- **Projeto Único**: Simples, todos os comandos são executados diretamente no diretório do projeto sem parâmetros adicionais.
- **Múltiplos Projetos**: Requer especificação de `-p` (projeto com o contexto) e `-s` (projeto de inicialização), pois a configuração e o contexto estão separados.

Com isso, você pode usar o `dotnet ef` em ambos os cenários de forma eficiente!
```


