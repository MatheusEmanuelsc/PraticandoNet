

---

# Resumo Completo: Testes Unitários com FluentAssertions

FluentAssertions é uma biblioteca que facilita a escrita de testes unitários em C#, proporcionando uma sintaxe fluente e legível. Abaixo, apresento um resumo dos principais métodos e conceitos utilizados com FluentAssertions, com exemplos explicativos e o uso do `Which`.

## Índice

1. [Introdução ao FluentAssertions](#introdução-ao-fluentassertions)
2. [Métodos Comuns](#métodos-comuns)
   - [Should()](#should)
   - [Be()](#be)
   - [BeEquivalentTo()](#beequivalentto)
   - [Contain()](#contain)
   - [HaveCount()](#havecount)
   - [Throw<TException>()](#throwtexception)
   - [NotBe()](#notbe)
   - [StartWith() e EndWith()](#startwith-e-endwith)
   - [BeGreaterThan() e BeLessThan()](#begreaterthan-e-belessthan)
   - [BeTrue() e BeFalse()](#betrue-e-befalse)
   - [BeNull() e NotBeNull()](#benull-e-notbenull)
   - [BeEmpty() e NotBeEmpty()](#beempty-e-notbeempty)
   - [Which](#which)
3. [Exemplos de Uso](#exemplos-de-uso)

## Introdução ao FluentAssertions

FluentAssertions é uma biblioteca que melhora a clareza e legibilidade dos testes unitários. Utilizando uma sintaxe fluente, ela permite escrever assertivas de maneira mais intuitiva, semelhante à linguagem natural.

## Métodos Comuns

### Should()

O método `Should()` inicia uma asserção fluente. Ele cria uma interface para continuar com asserções adicionais.

**Exemplo:**

```csharp
int resultado = 5;
resultado.Should().Be(5);
```

Aqui, estamos verificando que `resultado` deve ser igual a 5.

### Be()

O método `Be()` é usado para verificar se um valor é igual a outro.

**Exemplo:**

```csharp
int resultado = 5;
resultado.Should().Be(5);
```

Verifica se `resultado` é igual a 5.

### BeEquivalentTo()

Verifica se dois objetos são equivalentes, comparando suas propriedades relevantes.

**Exemplo:**

```csharp
var objeto1 = new { Nome = "João", Idade = 30 };
var objeto2 = new { Nome = "João", Idade = 30 };

objeto1.Should().BeEquivalentTo(objeto2);
```

Verifica se `objeto1` e `objeto2` têm as mesmas propriedades e valores.

### Contain()

Verifica se uma coleção ou string contém um valor específico.

**Exemplo com coleções:**

```csharp
var lista = new List<int> { 1, 2, 3 };
lista.Should().Contain(2);
```

Verifica se a lista contém o número 2.

**Exemplo com strings:**

```csharp
var texto = "Olá, mundo!";
texto.Should().Contain("mundo");
```

Verifica se a string contém a palavra "mundo".

### HaveCount()

Verifica o número de itens em uma coleção.

**Exemplo:**

```csharp
var lista = new List<int> { 1, 2, 3 };
lista.Should().HaveCount(3);
```

Verifica se a lista contém exatamente 3 itens.

### Throw<TException>()

Verifica se uma ação ou função lança uma exceção do tipo especificado.

**Exemplo:**

```csharp
Action act = () => throw new InvalidOperationException("Erro!");
act.Should().Throw<InvalidOperationException>();
```

Verifica se a ação lança uma `InvalidOperationException`.

### NotBe()

Verifica se um valor não é igual a outro.

**Exemplo:**

```csharp
int resultado = 5;
resultado.Should().NotBe(10);
```

Verifica se `resultado` não é igual a 10.

### StartWith() e EndWith()

Verificam se uma string começa (`StartWith()`) ou termina (`EndWith()`) com uma substring específica.

**Exemplo `StartWith()`:**

```csharp
var texto = "Olá, mundo!";
texto.Should().StartWith("Olá");
```

Verifica se a string começa com "Olá".

**Exemplo `EndWith()`:**

```csharp
var texto = "Olá, mundo!";
texto.Should().EndWith("mundo!");
```

Verifica se a string termina com "mundo!".

### BeGreaterThan() e BeLessThan()

Verificam se um valor é maior (`BeGreaterThan()`) ou menor (`BeLessThan()`) que outro.

**Exemplo `BeGreaterThan()`:**

```csharp
int valor = 10;
valor.Should().BeGreaterThan(5);
```

Verifica se `valor` é maior que 5.

**Exemplo `BeLessThan()`:**

```csharp
int valor = 10;
valor.Should().BeLessThan(20);
```

Verifica se `valor` é menor que 20.

### BeTrue() e BeFalse()

Verificam se um valor booleano é verdadeiro (`BeTrue()`) ou falso (`BeFalse()`).

**Exemplo `BeTrue()`:**

```csharp
bool condicao = true;
condicao.Should().BeTrue();
```

Verifica se `condicao` é verdadeiro.

**Exemplo `BeFalse()`:**

```csharp
bool condicao = false;
condicao.Should().BeFalse();
```

Verifica se `condicao` é falso.

### BeNull() e NotBeNull()

Verificam se um objeto é nulo (`BeNull()`) ou não nulo (`NotBeNull()`).

**Exemplo `BeNull()`:**

```csharp
object objeto = null;
objeto.Should().BeNull();
```

Verifica se `objeto` é nulo.

**Exemplo `NotBeNull()`:**

```csharp
object objeto = new object();
objeto.Should().NotBeNull();
```

Verifica se `objeto` não é nulo.

### BeEmpty() e NotBeEmpty()

Verificam se uma coleção ou string está vazia (`BeEmpty()`) ou não vazia (`NotBeEmpty()`).

**Exemplo `BeEmpty()`:**

```csharp
var lista = new List<int>();
lista.Should().BeEmpty();
```

Verifica se `lista` está vazia.

**Exemplo `NotBeEmpty()`:**

```csharp
var lista = new List<int> { 1 };
lista.Should().NotBeEmpty();
```

Verifica se `lista` não está vazia.

### Which

O método `Which` é usado para fazer asserções adicionais em um item específico de uma coleção. 

**Exemplo:**

```csharp
var lista = new List<Aluno>
{
    new Aluno { Nome = "João", Idade = 20 },
    new Aluno { Nome = "Maria", Idade = 22 }
};

lista.Should().ContainSingle(aluno => aluno.Nome == "Maria")
      .Which.Idade.Should().Be(22);
```

Neste exemplo:
1. **`ContainSingle(aluno => aluno.Nome == "Maria")`**: Verifica se há exatamente um aluno na lista com o nome "Maria".
2. **`Which`**: Refere-se ao aluno encontrado.
3. **`Idade.Should().Be(22)`**: Verifica se a idade do aluno encontrado é 22.

## Exemplos de Uso

Aqui estão alguns exemplos práticos utilizando os métodos do FluentAssertions:

1. **Verificar igualdade de valores:**

    ```csharp
    int resultado = 5;
    resultado.Should().Be(5);
    ```

2. **Verificar equivalência de objetos:**

    ```csharp
    var aluno1 = new Aluno { Nome = "João", Idade = 20 };
    var aluno2 = new Aluno { Nome = "João", Idade = 20 };
    aluno1.Should().BeEquivalentTo(aluno2);
    ```

3. **Verificar se uma string contém um valor:**

    ```csharp
    var texto = "Olá, mundo!";
    texto.Should().Contain("mundo");
    ```

4. **Verificar número de itens em uma lista:**

    ```csharp
    var lista = new List<int> { 1, 2, 3 };
    lista.Should().HaveCount(3);
    ```

5. **Verificar exceção lançada:**

    ```csharp
    Action act = () => throw new InvalidOperationException("Erro!");
    act.Should().Throw<InvalidOperationException>();
    ```

6. **Verificar se um valor não é igual a outro:**

    ```csharp
    int resultado = 5;
    resultado.Should().NotBe(10);
    ```

7. **Verificar o início e o fim de uma string:**

    ```csharp
    var texto = "Olá, mundo!";
    texto.Should().StartWith("Olá");
    texto.Should().EndWith("mundo!");
    ```

8. **Verificar se um valor é maior ou menor que outro:**

    ```csharp
    int valor =

 10;
    valor.Should().BeGreaterThan(5);
    valor.Should().BeLessThan(20);
    ```

9. **Verificar se um valor booleano é verdadeiro ou falso:**

    ```csharp
    bool condicao = true;
    condicao.Should().BeTrue();
    ```

10. **Verificar se um objeto é nulo ou não:**

    ```csharp
    object objeto = null;
    objeto.Should().BeNull();
    ```

11. **Verificar se uma lista está vazia:**

    ```csharp
    var lista = new List<int>();
    lista.Should().BeEmpty();
    ```

12. **Verificar condições adicionais com `Which`:**

    ```csharp
    var lista = new List<Aluno>
    {
        new Aluno { Nome = "João", Idade = 20 },
        new Aluno { Nome = "Maria", Idade = 22 }
    };

    lista.Should().ContainSingle(aluno => aluno.Nome == "Maria")
          .Which.Idade.Should().Be(22);
    ```

