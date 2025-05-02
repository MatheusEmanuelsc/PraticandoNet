using Artigos.Context;
using Artigos.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Contexto da aplicação (domínio principal)
builder.Services.AddDbContext<ArtigosDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// Contexto de Identity com MySQL
builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("IdentityConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("IdentityConnection"))
    ));


// Configuração do sistema Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
        // Políticas de senha
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    
        // Políticas de usuário
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
    
        // Bloqueio de conta
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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