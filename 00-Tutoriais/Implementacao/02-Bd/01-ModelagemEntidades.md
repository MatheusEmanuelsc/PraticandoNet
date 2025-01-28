### Índice  

1. [Introdução](#introducao)  
2. [Estrutura do Domínio](#estrutura-do-dominio)  
   - [Entidades](#entidades)  
     - [Classe Account](#classe-account)  
     - [Classe Customer](#classe-customer)  
     - [Classe Transaction](#classe-transaction)  
   - [Enums](#enums)  
     - [Enum AccountType](#enum-accounttype)  
     - [Enum TransactionType](#enum-transactiontype)  

---

### **Introdução** <a id="introducao"></a>  

Nesta etapa, configuraremos o domínio do sistema, que representa as tabelas do banco de dados. O domínio será organizado com as seguintes subpastas:  
- **Entities**: Para definir as classes que representam tabelas e seus relacionamentos.  
- **Enums**: Para armazenar enumeradores que auxiliam na padronização de tipos.

---

### **Estrutura do Domínio** <a id="estrutura-do-dominio"></a>  

#### **Entidades** <a id="entidades"></a>  

##### **Classe `Account`** <a id="classe-account"></a>  
```csharp
using Bank.Domain.Enums;

namespace Bank.Domain.Entities;

/// <summary>
/// Representa uma conta bancária no sistema.
/// </summary>
public class Account
{
    public long Id { get; set; } // Identificador único da conta.
    
    public long CustomerId { get; set; } // Relaciona a conta a um cliente.
    public Customer Customer { get; set; } = null!; // Propriedade de navegação para o cliente.

    public AccountType AccountType { get; set; } // Define o tipo da conta (Corrente ou Poupança).
    public decimal Balance { get; set; } = 0.0m; // Saldo inicial da conta.
    
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Data de criação da conta.
    public DateTime? UpdatedAt { get; set; } // Data da última atualização.
}
```
- **Propriedades**:
  - `Id`: Identificador único, usado como chave primária.
  - `CustomerId`: Chave estrangeira que relaciona a conta com um cliente.
  - `Customer`: Propriedade de navegação para facilitar o relacionamento com o cliente.
  - `AccountType`: Enum que especifica o tipo de conta (Corrente ou Poupança).
  - `Balance`: Representa o saldo da conta, inicializado como zero.
  - `CreatedAt` e `UpdatedAt`: Registram a data de criação e atualização da conta.

---

##### **Classe `Customer`** <a id="classe-customer"></a>  
```csharp
namespace Bank.Domain.Entities;

/// <summary>
/// Representa um cliente no sistema.
/// </summary>
public class Customer
{
    public long Id { get; set; } // Identificador único do cliente.
    public string Name { get; set; } = string.Empty; // Nome do cliente.
    public string Email { get; set; } = string.Empty; // Email do cliente.
    public string PhoneNumber { get; set; } = string.Empty; // Número de telefone do cliente.
    
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Data de criação do cliente.
    public DateTime? UpdatedAt { get; set; } // Data da última atualização.

    public ICollection<Account> Accounts { get; set; } = new List<Account>(); // Lista de contas associadas ao cliente.
}
```
- **Propriedades**:
  - `Id`: Identificador único, usado como chave primária.
  - `Name`, `Email`, `PhoneNumber`: Dados básicos do cliente.
  - `Accounts`: Representa o relacionamento 1:N, onde um cliente pode ter várias contas.

---

##### **Classe `Transaction`** <a id="classe-transaction"></a>  
```csharp
using Bank.Domain.Enums;

namespace Bank.Domain.Entities;

/// <summary>
/// Representa uma transação realizada em uma conta.
/// </summary>
public class Transaction
{
    public long Id { get; set; } // Identificador único da transação.

    public long AccountId { get; set; } // Chave estrangeira para a conta associada à transação.
    public Account Account { get; set; } = null!; // Propriedade de navegação para a conta.

    public TransactionType TransactionType { get; set; } // Define o tipo de transação (Depósito, Saque ou Transferência).
    public decimal Amount { get; set; } = 0.0m; // Valor da transação.
    public DateTime Date { get; set; } = DateTime.Now; // Data da transação.

    public long? DestinationAccountId { get; set; } // Chave estrangeira opcional para a conta de destino (no caso de transferências).
    public Account? DestinationAccount { get; set; } // Propriedade de navegação para a conta de destino.
}
```
- **Propriedades**:
  - `Id`: Identificador único da transação.
  - `AccountId` e `Account`: Relacionam a transação à conta de origem.
  - `TransactionType`: Enum que define o tipo de transação.
  - `Amount`: Valor da transação.
  - `DestinationAccountId` e `DestinationAccount`: Relacionam a transação à conta de destino, quando aplicável.

---

#### **Enums** <a id="enums"></a>  

##### **Enum `AccountType`** <a id="enum-accounttype"></a>  
```csharp
namespace Bank.Domain.Enums;

/// <summary>
/// Define os tipos de conta disponíveis.
/// </summary>
public enum AccountType
{
    Checking = 1, // Conta corrente.
    Savings = 2   // Conta poupança.
}
```
- **Descrição**: Este enum categoriza o tipo de conta em Corrente (1) ou Poupança (2).

---

##### **Enum `TransactionType`** <a id="enum-transactiontype"></a>  
```csharp
namespace Bank.Domain.Enums;

/// <summary>
/// Define os tipos de transação disponíveis.
/// </summary>
public enum TransactionType
{
    Deposit = 1,  // Depósito.
    Withdraw = 2, // Saque.
    Transfer = 3  // Transferência.
}
```
- **Descrição**: Este enum categoriza as transações em Depósito (1), Saque (2) e Transferência (3).

---

Com essas entidades e enums configurados, temos a base para criar e mapear as tabelas do banco de dados. A próxima etapa será configurar o **Entity Framework Core** para mapear essas classes e realizar migrações.