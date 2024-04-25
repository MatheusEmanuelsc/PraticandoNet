## Conexão com Banco de Dados em Diferentes Tecnologias

**Introdução**

Este arquivo .md fornece instruções para configurar conexões com bancos de dados em diferentes tecnologias, incluindo MySQL, PostgreSQL, SQL Server e SQLite. O objetivo é que você possa copiar e colar o código, alterando apenas algumas variáveis de acordo com suas necessidades.

**Pré-requisitos**

* Ter o software de banco de dados instalado e configurado
* Conhecer as credenciais de acesso ao banco de dados (usuário, senha, nome do banco de dados, etc.)

**Importante:**

No exemplo anterior, todas as strings de conexão estavam utilizando a mesma variável `mySqlConnection`. Isso pode levar a erros caso você esteja utilizando mais de um banco de dados em seu projeto. Para evitar esse problema, vamos utilizar variáveis diferentes para cada tipo de banco de dados.

**Conexão com MySQL**

**String de Conexão (appsettings.json):**

```json
"ConnectionStrings": {
  "MySQLConnection": "server=localhost;database=CatalogoDB;uid=root;pwd=senha_aqui"
}
```

**Código de Conexão (programa C#):**

```c#
var mysqlConnection = builder.Configuration.GetConnectionString("MySQLConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
  options.UseMySql(mysqlConnection, ServerVersion.AutoDetect(mysqlConnection));
});
```

**Variáveis a serem alteradas:**

* `server`: Endereço do servidor MySQL
* `database`: Nome do banco de dados MySQL
* `uid`: Usuário MySQL
* `pwd`: Senha do usuário MySQL

**Conexão com PostgreSQL**

**String de Conexão (appsettings.json):**

```json
"ConnectionStrings": {
  "PostgreSQLConnection": "Host=localhost;Database=CatalogoDB;Username=postgres;Password=senha_aqui"
}
```

**Código de Conexão (programa C#):**

```c#
var postgresqlConnection = builder.Configuration.GetConnectionString("PostgreSQLConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
  options.UseNpgsql(postgresqlConnection);
});
```

**Variáveis a serem alteradas:**

* `Host`: Endereço do servidor PostgreSQL
* `Database`: Nome do banco de dados PostgreSQL
* `Username`: Usuário PostgreSQL
* `Password`: Senha do usuário PostgreSQL

**Conexão com SQL Server**

**String de Conexão (appsettings.json):**

```json
"ConnectionStrings": {
  "SqlServerConnection": "Data Source=localhost;Initial Catalog=CatalogoDB;Integrated Security=True"
}
```

**Código de Conexão (programa C#):**

```c#
var sqlServerConnection = builder.Configuration.GetConnectionString("SqlServerConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
  options.UseSqlServer(sqlServerConnection);
});
```

**Variáveis a serem alteradas:**

* `Data Source`: Endereço do servidor SQL Server
* `Initial Catalog`: Nome do banco de dados SQL Server
* **Opcional:** `Integrated Security=True` (autenticação do Windows) ou `User ID=usuario_sql;Password=senha_sql` (autenticação SQL Server)

**Conexão com SQLite**

**String de Conexão (appsettings.json):**

```json
"ConnectionStrings": {
  "SQLiteConnection": "Data Source=CatalogoDB.sqlite"
}
```

**Código de Conexão (programa C#):**

```c#
var sqliteConnection = builder.Configuration.GetConnectionString("SQLiteConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
  options.UseSqlite(sqliteConnection);
});
```

**Variáveis a serem alteradas:**

* `Data Source`: Caminho para o arquivo SQLite

**Observações**

* Certifique-se de instalar os pacotes NuGet necessários para cada tecnologia de banco de dados.
* As configurações de conexão podem variar de acordo com a sua versão do banco de dados e as suas necessidades específicas.
* Consulte a documentação oficial de cada tecnologia para obter mais detalhes sobre as opções de configuração disponíveis.

**Exemplo Completo**

```markdown
## Conexão com Banco de Dados

Este arquivo fornece instruções para configurar conexões com bancos de dados em diferentes tecnologias.

**Pré-requisitos:**

* Ter o software de banco de dados instalado e configurado
* Conhecer as credenciais de acesso ao banco de dados (usuário, senha, nome do banco de dados, etc.)

