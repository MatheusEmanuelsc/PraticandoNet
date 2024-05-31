# .NET JSON Serializer e Deserializer para Entidades Públicas

## Índice
1. [Introdução](#introdução)
2. [Serialização](#serialização)
3. [Desserialização](#desserialização)
4. [Métodos para Ignorar Propriedades](#métodos-para-ignorar-propriedades)
    - [Atributo `[JsonIgnore]`](#atributo-jsonignore)
    - [Atributo `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`](#atributo-jsonignorecondition--jsonignoreconditionwhenwritingnull)
    - [Atributo `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]`](#atributo-jsonignorecondition--jsonignoreconditionwhenwritingdefault)
    - [Ignorar Valores Nulos Globalmente](#ignorar-valores-nulos-globalmente)
    - [Ignorar Valores Padrão Globalmente](#ignorar-valores-padrão-globalmente)
    - [Atributo `[JsonIgnore(Condition = JsonIgnoreCondition.Always)]`](#atributo-jsonignorecondition--jsonignoreconditionalways)
    - [Ignorar Propriedades Dinamicamente](#ignorar-propriedades-dinamicamente)

## Introdução

O .NET oferece suporte robusto para a serialização e desserialização de objetos JSON através da biblioteca `System.Text.Json`. Esse processo é essencial para a comunicação com APIs, armazenamento de configurações e manipulação de dados em aplicações modernas.

## Serialização

A serialização é o processo de converter um objeto C# em uma string JSON. Isso é útil para enviar dados através de uma rede, armazenar em um arquivo ou interagir com APIs que aceitam JSON.

### Exemplo Básico

```csharp
using System.Text.Json;

public class PublicEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
}

var entity = new PublicEntity
{
    Id = 1,
    Name = "Entity Name",
    CreatedDate = DateTime.UtcNow
};

string jsonString = JsonSerializer.Serialize(entity);
Console.WriteLine(jsonString);
```

## Desserialização

A desserialização é o processo de converter uma string JSON de volta em um objeto C#. Isso é essencial quando se recebe dados de uma API ou se carrega dados de um arquivo JSON.

### Exemplo Básico

```csharp
string jsonString = "{\"identifier\":1,\"entity_name\":\"Entity Name\",\"created_at\":\"2024-05-30T00:00:00Z\"}";

var entity = JsonSerializer.Deserialize<PublicEntity>(jsonString);
Console.WriteLine($"Id: {entity.Id}, Name: {entity.Name}, CreatedDate: {entity.CreatedDate}");
```

## Métodos para Ignorar Propriedades

### Atributo `[JsonIgnore]`

Ignora uma propriedade específica durante a serialização e a desserialização.

```csharp
public class PublicEntity
{
    public int Id { get; set; }
    
    [JsonIgnore]
    public string Name { get; set; }
    
    public DateTime CreatedDate { get; set; }
}
```

### Atributo `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`

Ignora uma propriedade somente quando seu valor é `null` durante a serialização.

```csharp
public class PublicEntity
{
    public int Id { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; set; }
    
    public DateTime CreatedDate { get; set; }
}
```

### Atributo `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]`

Ignora uma propriedade quando seu valor é o valor padrão para o tipo de dados.

```csharp
public class PublicEntity
{
    public int Id { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Name { get; set; }
    
    public DateTime CreatedDate { get; set; }
}
```

### Ignorar Valores Nulos Globalmente

Configura o comportamento de serialização globalmente para ignorar valores nulos.

```csharp
var options = new JsonSerializerOptions
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

string jsonString = JsonSerializer.Serialize(entity, options);
```

### Ignorar Valores Padrão Globalmente

Configura o comportamento de serialização globalmente para ignorar valores padrão.

```csharp
var options = new JsonSerializerOptions
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
};

string jsonString = JsonSerializer.Serialize(entity, options);
```

### Atributo `[JsonIgnore(Condition = JsonIgnoreCondition.Always)]`

Sempre ignora uma propriedade durante a serialização e a desserialização.

```csharp
public class PublicEntity
{
    public int Id { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string Name { get; set; }
    
    public DateTime CreatedDate { get; set; }
}
```

### Ignorar Propriedades Dinamicamente

Ignora propriedades dinamicamente usando `JsonSerializerOptions` e um `JsonConverter`.

```csharp
public class IgnorePropertiesConverter<T> : JsonConverter<T>
{
    private readonly HashSet<string> _propertiesToIgnore;

    public IgnorePropertiesConverter(HashSet<string> propertiesToIgnore)
    {
        _propertiesToIgnore = propertiesToIgnore;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<T>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var jsonObject = JsonSerializer.SerializeToElement(value, options);
        foreach (var property in _propertiesToIgnore)
        {
            jsonObject.TryGetProperty(property, out _);
        }
        jsonObject.WriteTo(writer);
    }
}

var options = new JsonSerializerOptions();
options.Converters.Add(new IgnorePropertiesConverter<PublicEntity>(new HashSet<string> { "Name" }));

string jsonString = JsonSerializer.Serialize(entity, options);
```

## Conclusão

A serialização e desserialização de JSON em .NET são processos críticos para o desenvolvimento moderno de software. Utilizando `System.Text.Json`, desenvolvedores podem controlar finamente quais propriedades são incluídas no JSON resultante, tanto globalmente quanto por propriedade individual, facilitando a comunicação eficiente e flexível com APIs e armazenamento de dados.