

```markdown
# Comandos Principais para Migra��es no .NET 8

## Criar uma Migra��o

### Adicionar uma Nova Migra��o
```bash
dotnet ef migrations add <NomeDaMigracao>
```

## Aplicar Migra��es

### Aplicar Migra��es ao Banco de Dados
```bash
dotnet ef database update
```

## Gerenciar Mudan�as no Modelo

### Adicionar uma Nova Migra��o Ap�s Mudan�as no Modelo
```bash
dotnet ef migrations add <NomeDaNovaMigracao>
```

### Atualizar o Banco de Dados com a Nova Migra��o
```bash
dotnet ef database update
```

## Reverter Migra��es

### Reverter para uma Migra��o Anterior
```bash
dotnet ef database update <NomeDaMigracaoAnterior>
```

## Remover uma Migra��o

### Remover a �ltima Migra��o
```bash
dotnet ef migrations remove
```

## Ferramentas e Op��es Adicionais

### Listar Todas as Migra��es
```bash
dotnet ef migrations list
```

### Gerar um Script SQL das Migra��es
```bash
dotnet ef migrations script
```

## Conclus�o

Este guia cobre os comandos essenciais para criar, aplicar, gerenciar e reverter migra��es no .NET 8 usando o Entity Framework Core. Use esses comandos para manter seu esquema de banco de dados sincronizado com o modelo de dados de sua aplica��o.
```

