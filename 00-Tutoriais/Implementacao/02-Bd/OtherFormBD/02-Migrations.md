

```markdown
# Comandos Principais para Migrações no .NET 8

## Criar uma Migração

### Adicionar uma Nova Migração
```bash
dotnet ef migrations add <NomeDaMigracao>
```

## Aplicar Migrações

### Aplicar Migrações ao Banco de Dados
```bash
dotnet ef database update
```

## Gerenciar Mudanças no Modelo

### Adicionar uma Nova Migração Após Mudanças no Modelo
```bash
dotnet ef migrations add <NomeDaNovaMigracao>
```

### Atualizar o Banco de Dados com a Nova Migração
```bash
dotnet ef database update
```

## Reverter Migrações

### Reverter para uma Migração Anterior
```bash
dotnet ef database update <NomeDaMigracaoAnterior>
```

## Remover uma Migração

### Remover a Última Migração
```bash
dotnet ef migrations remove
```

## Ferramentas e Opções Adicionais

### Listar Todas as Migrações
```bash
dotnet ef migrations list
```

### Gerar um Script SQL das Migrações
```bash
dotnet ef migrations script
```

## Conclusão

Este guia cobre os comandos essenciais para criar, aplicar, gerenciar e reverter migrações no .NET 8 usando o Entity Framework Core. Use esses comandos para manter seu esquema de banco de dados sincronizado com o modelo de dados de sua aplicação.
```

