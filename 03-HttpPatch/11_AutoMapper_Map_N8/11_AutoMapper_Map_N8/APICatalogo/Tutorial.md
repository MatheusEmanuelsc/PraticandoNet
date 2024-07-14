Tutorial Atualização parcial

Etapa 1 adicione os pacotes

    dotnet add package Microsoft.AspNetCore.JsonPatch

        permite implmentar operações de patch parcial em recursos restfull por meio do metodo patch

        oferece suporte a aplicações de atualizações parciais em objetos complexos como modelos de dominio ou entidades de banco de dados



    dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson

        Habilita o suporte ao Json patch na aspnet core  web api
        fornece um parser e um serializador para o formato patch json

Etapa 2  configure o program


    adicione nesse bloco de codigo

    // Add services to the container.
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add(typeof(ApiExceptionFilter));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    }).AddNewtonsoftJson();

    no caso vc adciona isso no final como no exemplo  .AddNewtonsoftJson();

Etapa 3 vc  agora vai criar 2x Dto um response e request
