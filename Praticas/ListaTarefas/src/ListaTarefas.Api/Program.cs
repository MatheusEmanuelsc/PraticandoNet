using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using ListaTarefas.Api.Context;
using ListaTarefas.Api.Entities;
using ListaTarefas.Api.Repository;
using ListaTarefas.Api.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {

        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }).AddNewtonsoftJson();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<TarefaCreateDtoValidator>();

    
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Configurações de senha
        options.Password.RequireDigit = true; // Exige pelo menos um dígito
        options.Password.RequiredLength = 8; // Mínimo de 8 caracteres
        options.Password.RequireNonAlphanumeric = true; // Exige caracteres especiais (ex.: @, #)
        options.Password.RequireUppercase = true; // Exige letras maiúsculas
        options.Password.RequireLowercase = true; // Exige letras minúsculas
        // options.Password.RequiredUniqueChars = 4; // Exige pelo menos 4 caracteres únicos

        // Configurações de bloqueio de conta
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Bloqueio por 5 minutos
        options.Lockout.MaxFailedAccessAttempts = 5; // Máximo de 5 tentativas falhas
        options.Lockout.AllowedForNewUsers = true; // Bloqueio habilitado para novos usuários

        // Configurações de usuário
        options.User.RequireUniqueEmail = true; // Exige e-mails únicos
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Caracteres permitidos no nome de usuário
        options.SignIn.RequireConfirmedAccount =
            false; // Desativa confirmação de conta (pode ser ativado se necessário)

        // Configurações de token para redefinição de senha
        options.Tokens.PasswordResetTokenProvider =
            TokenOptions.DefaultProvider; // Provedor padrão para tokens de redefinição
        options.Tokens.ChangeEmailTokenProvider =
            TokenOptions.DefaultEmailProvider; // Provedor para tokens de alteração de e-mail
    })
    .AddEntityFrameworkStores<AppDbContext>() // Integra com Entity Framework
    .AddDefaultTokenProviders(); // Provedores padrão para tokens
    // .AddErrorDescriber<CustomIdentityErrorDescriber>(); // Personaliza mensagens de erro


    var configuration = builder.Configuration;

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Define JWT como esquema padrão
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Define JWT para desafios
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, // Valida o emissor
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true, // Valida a audiência
                ValidAudience = configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true, // Valida a chave de assinatura
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                ValidateLifetime = true, // Verifica expiração
                ClockSkew = TimeSpan.Zero // Sem tolerância para expiração
            };
        });

    builder.Services.AddAuthorization(); // Habilita serviços de autorização
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();