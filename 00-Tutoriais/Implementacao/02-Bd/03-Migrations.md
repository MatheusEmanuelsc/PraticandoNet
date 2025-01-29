

# 📌 **Migrations no .NET 8 com Clean Architecture**  

## 📖 **Índice**  
1. [O que são Migrations?](#o-que-são-migrations)  
2. [Pré-requisitos](#pré-requisitos)  
3. [Criando uma Migration](#criando-uma-migration)  
4. [Aplicando a Migration](#aplicando-a-migration)  
5. [Removendo uma Migration](#removendo-uma-migration)  
6. [Atualizando o Banco de Dados](#atualizando-o-banco-de-dados)  
7. [Resolvendo Erros Comuns](#resolvendo-erros-comuns)  

---

## 📌 **1. O que são Migrations?**  
Migrations são uma forma de versionar a estrutura do banco de dados no **Entity Framework Core**. Elas permitem criar, modificar e deletar tabelas de forma controlada, evitando problemas em ambientes de desenvolvimento e produção.  

---

## ⚙ **2. Pré-requisitos**  
Antes de começar, certifique-se de que:  
✅ O **Entity Framework Core** está instalado no projeto.  
✅ O banco de dados está configurado no `CashBank.Infrastructure`.  
✅ Você tem o **.NET CLI** e o **EF Core CLI** instalados:  

```bash
dotnet tool install --global dotnet-ef
```

Se já estiver instalado, atualize com:

```bash
dotnet tool update --global dotnet-ef
```

---

## 🚀 **3. Criando uma Migration**  
Para gerar uma migration no Clean Architecture, use o seguinte comando:  

```bash
dotnet ef migrations add InitialMigration --project CashBank.Infrastructure --startup-project CashBank.Api
```

### 🛠 **Explicação do Comando**  
- `dotnet ef migrations add InitialMigration` → Cria a migration chamada `InitialMigration`.  
- `--project CashBank.Infrastructure` → Indica o projeto onde o **DbContext** está localizado.  
- `--startup-project CashBank.Api` → Especifica o projeto principal que executa a aplicação.  

Se o comando for bem-sucedido, uma pasta `Migrations/` será criada dentro de `CashBank.Infrastructure`, contendo:  
✅ **InitialMigration.cs** (descrição das alterações).  
✅ **InitialMigration.Designer.cs** (metadados).  
✅ **CashBankModelSnapshot.cs** (estado atual do banco).  

---

## 🔄 **4. Aplicando a Migration**  
Para aplicar as alterações ao banco de dados, use:  

```bash
dotnet ef database update --project CashBank.Infrastructure --startup-project CashBank.Api
```

Isso criará as tabelas no banco de dados configurado.

---

## ❌ **5. Removendo uma Migration**  
Caso precise remover a última migration antes de aplicá-la ao banco, use:  

```bash
dotnet ef migrations remove --project CashBank.Infrastructure --startup-project CashBank.Api
```

Isso excluirá a migration mais recente, permitindo que você a recrie com ajustes.

---

## 🔄 **6. Atualizando o Banco de Dados**  
Sempre que adicionar novas propriedades ou classes ao seu modelo, crie uma nova migration:  

```bash
dotnet ef migrations add NovaMigration --project CashBank.Infrastructure --startup-project CashBank.Api
```

E aplique as mudanças com:

```bash
dotnet ef database update --project CashBank.Infrastructure --startup-project CashBank.Api
```

---

## 🚨 **7. Resolvendo Erros Comuns**  

### ❌ **"Unable to create an object of type 'AppDbContext'"**  
🔹 Certifique-se de que o `CashBank.Infrastructure` contém um **DbContextFactory** ou que o `DbContext` está corretamente configurado no `Program.cs` do `CashBank.Api`.  

### ❌ **"The migration was already applied"**  
🔹 Verifique se o banco de dados já possui a migration executando:  

```bash
dotnet ef migrations list --project CashBank.Infrastructure
```

🔹 Se necessário, reverta para um estado anterior:

```bash
dotnet ef database update LastGoodMigration --project CashBank.Infrastructure
```

---

## 🎯 **Conclusão**  
Agora você sabe como criar, aplicar e gerenciar migrations no .NET 8 usando Clean Architecture. 🚀 Se precisar adicionar mais tabelas ou alterar o banco, basta repetir o processo!