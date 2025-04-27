

```md
# 🧱 Estrutura com 5 Camadas no ASP.NET Core

## 📌 O que é?

A estrutura de **5 camadas** separa ainda mais as responsabilidades do sistema, dividindo a tradicional camada `Infrastructure` em duas partes:

- 🔸 **`Infrastructure`**: Serviços externos (email, APIs de terceiros, autenticação externa, logs, etc.)
- 🔸 **`Persistence`**: Tudo relacionado a banco de dados (EF Core, repositórios, DbContext, Unit of Work)

---

## 🧩 Camadas do Modelo de 5 Camadas

| Camada              | Responsabilidade principal                                                              |
|---------------------|------------------------------------------------------------------------------------------|
| `API`               | Interface com o mundo externo (controllers, Program.cs, middlewares, etc.)              |
| `Application`       | Casos de uso da aplicação (serviços, DTOs, validações, regras de negócio da aplicação)  |
| `Domain`            | Núcleo do domínio (entidades, interfaces, regras de negócio puras)                      |
| `Infrastructure`    | Serviços externos (envio de email, logs, comunicação com APIs externas)                 |
| `Persistence`       | Persistência de dados (EF Core, DbContext, repositórios, migrations, UoW)               |

---

## 🔄 Comparativo: 4 Camadas vs 5 Camadas

| Característica               | Estrutura com 4 Camadas                     | Estrutura com 5 Camadas                     |
|-----------------------------|---------------------------------------------|---------------------------------------------|
| Banco de dados (EF Core)    | Dentro da `Infrastructure`                 | Em `Persistence`                            |
| Serviços externos (e-mail)  | Também na `Infrastructure`                 | Em `Infrastructure`                         |
| Separação de responsabilidades | Menos granular                            | Mais clara e modular                        |
| Manutenção/escalabilidade   | Boa, mas pode ficar confuso com o tempo    | Excelente para projetos grandes             |
| Complexidade inicial        | Menor                                       | Um pouco maior (mais projetos)              |
| Ideal para                  | Projetos pequenos e médios                 | Projetos grandes e de longo prazo           |

---

## ✅ Quando Usar 5 Camadas?

| Situação                                         | Recomendação              |
|--------------------------------------------------|---------------------------|
| Projeto simples (CRUD básico, poucos serviços)   | Use 4 camadas             |
| Projeto médio, mas com poucos serviços externos  | Use 4 ou 5 (ambos ok)     |
| Projeto grande/empresarial com muitos domínios   | Use 5 camadas             |
| Uso intenso de serviços externos + banco         | Use 5 camadas             |
| Projeto com foco em testes e manutenção escalável| Use 5 camadas             |

---

## 🎯 Vantagens da Separação entre Infrastructure e Persistence

- 🔹 Facilita testes unitários
- 🔹 Código mais limpo e modular
- 🔹 Adoção mais fácil de microserviços no futuro
- 🔹 Especialização: times podem trabalhar em partes diferentes sem conflito
- 🔹 Permite usar diferentes bancos (ex: Mongo, Redis) separados de outros serviços externos

---

## 📦 Conclusão

A estrutura com 5 camadas é **ideal para projetos complexos ou corporativos**, onde clareza, separação de responsabilidades e facilidade de manutenção são essenciais. Para projetos menores ou MVPs, o modelo de 4 camadas ainda é totalmente válido por ser mais simples de montar e manter no início.

```

