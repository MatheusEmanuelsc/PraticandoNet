# Resumo: Modelagem de Projeto com DDD

## Índice
1. [Introdução](#introdução)  
2. [Etapa 1: Divisão de Camadas com DDD](#etapa-1-divisão-de-camadas-com-ddd)  
   - [Projeto API](#projeto-api)  
   - [Projeto Application](#projeto-application)  
   - [Projeto Communication](#projeto-communication)  
   - [Projeto Exception](#projeto-exception)  
   - [Projeto Domain](#projeto-domain)  
   - [Projeto Infrastructure (Infraestrutura)](#projeto-infrastructure)  
3. [Etapa 2: Criação do Projeto](#etapa-2-criação-do-projeto)  

---

## Introdução

Este guia detalha como modelar um projeto baseado no padrão DDD (Domain-Driven Design), dividindo as camadas em projetos independentes para garantir modularidade e organização. Ele também aborda o processo de criação da solução no Visual Studio ou Rider.

---

## Etapa 1: Divisão de Camadas com DDD

### **Projeto API**
- **Tipo:** Projeto principal da Web API.  
- **Responsabilidade:** Receber requisições e retornar respostas.  
- **Dependências:**  
  - `Communication`  
  - `Exception`  
  - `Application`  
  - `Infrastructure` (necessário para a injeção de dependência)  

**Nota:** Apesar de, na prática, a API precisar acessar a infraestrutura, isso não é recomendado no conceito de DDD.

---

### **Projeto Application**
- **Tipo:** Biblioteca de classes.  
- **Responsabilidade:** Contém as regras de negócio.  
- **Dependências:**  
  - `Communication`  
  - `Exception`  
  - `Domain`  

---

### **Projeto Communication**
- **Tipo:** Biblioteca de classes.  
- **Responsabilidade:** Armazena classes para requisições (Request) e respostas (Response).  
- **Dependência:** Projeto independente que não precisa conhecer a existência de outros projetos.

---

### **Projeto Exception**
- **Tipo:** Biblioteca de classes.  
- **Responsabilidade:** Centraliza os arquivos para tradução de erros.  
- **Dependência:** Projeto independente que não precisa conhecer a existência de outros projetos.

---

### **Projeto Domain**
- **Tipo:** Biblioteca de classes.  
- **Responsabilidade:** Armazena elementos comuns a todo o projeto, como contratos e entidades.  
- **Dependência:** Projeto independente que não precisa conhecer a existência de outros projetos.

---

### **Projeto Infrastructure (Infraestrutura)**
- **Tipo:** Biblioteca de classes.  
- **Responsabilidade:** Executa operações relacionadas ao banco de dados.  
- **Dependências:**  
  - `Domain`  

---

## Etapa 2: Criação do Projeto

### **Passo 1: Configuração Inicial**
1. No Visual Studio ou Rider, crie uma **solução vazia** (`Blank Solution`).  
2. Dentro da solução, crie duas pastas principais:  
   - `src`: Para o código-fonte.  
   - `test`: Para os testes.  

**Observação:** Após criar as pastas na solução, é necessário recriá-las manualmente no sistema de arquivos para que fiquem visíveis.

---

### **Passo 2: Organização dos Projetos**
1. Organize os projetos dentro da pasta `src` conforme o modelo da [Etapa 1](#etapa-1-divisão-de-camadas-com-ddd).  
   **ATENÇÃO:** Certifique-se de que os projetos sejam criados dentro da pasta `src`.  
2. Adicione as dependências necessárias clicando com o botão direito sobre o projeto e acessando **Dependencies**.  
3. Exclua as classes geradas automaticamente que não forem necessárias.

---

Este modelo garante uma estrutura organizada e alinhada com as boas práticas de DDD, facilitando o desenvolvimento e a manutenção do sistema.

