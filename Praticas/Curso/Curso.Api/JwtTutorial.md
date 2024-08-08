parte 1

	Etapa 1

	 instalar pacote

	 dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer


	Etapa 2

		 juaste o program adc     
			 builder.Services.AddAuthorization();
			 builder.Services.AddAuthentication("bearer").AddJwtBearer();

			pode remover isso se quiser

			app.UseAuthorization();

	 etapa 3 proteger os endpoints usando o atributo: [Authorize]

		ex:
			 [HttpGet("{id:int}", Name = "ObterAluno")]
			[Authorize]
			public async Task<ActionResult<AlunoDto>> GetAluno(int id)
			{
				var aluno = await _unitOfWork.AlunoRepository.GetAsync(a => a.AlunoId == id);
				if (aluno == null) { NotFound(); }
				var alunoDto = _mapper.Map<AlunoDto>(aluno);
				return Ok(alunoDto);

			}

	 etapa 4(Opicional)  gerar token de autorização

	 com isso podemos gerar e gerenciar rokens jwt usando a ferramenta dotnet user-jwt
	 user-jwt create para criar um token

		Aqui está a lista de comandos disponíveis no `dotnet user-jwt` junto com um pequeno resumo do que cada um faz:

	### Lista de Comandos `dotnet user-jwt`

	1. **add**
	   - **Resumo**: Adiciona um token JWT a um projeto .NET. Este comando é útil para adicionar rapidamente um token JWT para fins de desenvolvimento e teste.
	   - **Uso**: `dotnet user-jwt add`

	2. **remove**
	   - **Resumo**: Remove um token JWT de um projeto .NET. Este comando é útil para limpar tokens de desenvolvimento quando eles não são mais necessários.
	   - **Uso**: `dotnet user-jwt remove`

	3. **list**
	   - **Resumo**: Lista todos os tokens JWT associados ao projeto. Este comando ajuda a visualizar todos os tokens gerados e suas informações.
	   - **Uso**: `dotnet user-jwt list`

	4. **clear**
	   - **Resumo**: Remove todos os tokens JWT associados ao projeto. Útil para fazer uma limpeza geral de tokens de desenvolvimento.
	   - **Uso**: `dotnet user-jwt clear`

	5. **print**
	   - **Resumo**: Imprime as informações detalhadas de um token JWT específico. Este comando é útil para depuração e verificação das informações contidas no token.
	   - **Uso**: `dotnet user-jwt print --id <token-id>`

	Esses comandos são projetados para facilitar o gerenciamento de tokens JWT durante o desenvolvimento de aplicações ASP.NET Core, proporcionando uma forma rápida e eficiente de adicionar, listar, remover e inspecionar tokens JWT.




 

	 ai so usar o token gerado para acessar os endpoints protegidos


Parte 2 Identity

	Uitlizamos ele vai nos permites gerar as tabelas automaticamente para  gerenciar o tokens


	etapa 1
		
		adicione o pacote

			dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore

	Etapa 2 

		Modifique o context

			 public class AppDbContext : IdentityDbContext
		
		agora ele Herda Identity não context

	Etapa 3 

		Ajuste o Program

			builder.Services.AddIdentity<IdentityUser, IdentityRole>().
    AddEntityFrameworkStores<AppDbContext>().
    AddDefaultTokenProviders();

	Etapa 4 

		Aplicar o migrations

parte 3  jwt Beary 

	Etapa 1 adicione pacotes

		dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

	Etapa 2  ajuste appsetigs

		"Jwt": {
					"ValidAudience": "http://localhost:7066",
					"ValidIssuer": "http//localhost:5066",
					"SecretKey": "Minha@Super#Secreta&Chave*Priavada!2024%",
					"TokenValidityInMinutes": 30,
					"RefreshTokenValidityInMinutes": 60
				}
		Atenção em produção vc não pode deixar desse jeito

	Etapa 3 ajustando program

		var secretKey = builder.Configuration["JWT:SecretKey"]
                   ?? throw new ArgumentException("Invalid secret key!!");

			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(options =>
			{
				options.SaveToken = true;
				options.RequireHttpsMetadata = false;
				options.TokenValidationParameters = new TokenValidationParameters()
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ClockSkew = TimeSpan.Zero,
					ValidAudience = builder.Configuration["JWT:ValidAudience"],
					ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
					IssuerSigningKey = new SymmetricSecurityKey(
									   Encoding.UTF8.GetBytes(secretKey))
				};
			});

parte 4 criando e ajustando refresh token

	etapa 1  crie a classe
		
		using Microsoft.AspNetCore.Identity;

		namespace Curso.Api.Models
		{
			public class ApplicationUser: IdentityUser
			{
				public string? RefreshToken { get; set; }
				public DateTime RefreshTokenExpiryTime { get; set; }                                        
			}
		}


	Etapa 2 Ajuste o context

			 public class AppDbContext : IdentityDbContext<ApplicationUser>
		{
        
			public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
			{
			}

			public DbSet<Disciplina>? Disciplinas { get; set; }
			public DbSet<Aluno>? Alunos { get; set; }


			protected override void OnModelCreating(ModelBuilder builder)
			{
				base.OnModelCreating(builder);
			}
		
	Etapa 3 Ajuste o program

		builder.Services.AddIdentity<ApplicationUser, IdentityRole>().
		AddEntityFrameworkStores<AppDbContext>().
		AddDefaultTokenProviders();

	
		 Aplique a migrations

Parte 5 Criar Dtos para gerenciar o login o registro e o token

	Etapa 1 criar o dto do login

		 public class LoginModel
		{
			[Required(ErrorMessage ="User Name is required")]
			public string? UserName { get; set; }
			[Required(ErrorMessage = "User Name is required")]
			public string? Password { get; set; }
		}

	Etapa 2 criar o registro
		
		 public class RegisterModel
		{
			[Required(ErrorMessage = "User Name is required")]
			public string?  Username { get; set; }
			[Required(ErrorMessage = "User Name is required")]
			public string? Email { get; set; }
			[Required(ErrorMessage = "User Name is required")]
			public string? Password { get; set; }
		}

	Etapa 3 Criar model token

		    public class TokenModel
			{
				public string?  AcessToken { get; set; }
				public string? RefreshToken { get; set; }
			}

	Etapa 4 criar o response

			public class Response
			{
				  public string? Status { get; set; }
				  public string? Message { get; set; }
       
			}

Parte 5 Geração token

	Etapa 1 criei  a interface  do token

		 public interface ITokenService
		{
			JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims,
												 IConfiguration _config);
			string GenerateRefreshToken();

			ClaimsPrincipal GetPrincipalFromExpiredToken(string token,
														 IConfiguration _config);
		}

	Etapa 2  impmentação da interface

		 public class TokenService : ITokenService
    {
        public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration _config)
        {
            var key = _config.GetSection("JWT").GetValue<string>("SecretKey") ??
               throw new InvalidOperationException("Invalid secret Key");

            var privateKey = Encoding.UTF8.GetBytes(key);

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey),
                                     SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_config.GetSection("JWT")
                                                    .GetValue<double>("TokenValidityInMinutes")),
                Audience = _config.GetSection("JWT")
                                  .GetValue<string>("ValidAudience"),
                Issuer = _config.GetSection("JWT").GetValue<string>("ValidIssuer"),
                SigningCredentials = signingCredentials
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            return token;
        }

        public string GenerateRefreshToken()
        {
            var secureRandomBytes = new byte[128];

            using var randomNumberGenerator = RandomNumberGenerator.Create();

            randomNumberGenerator.GetBytes(secureRandomBytes);

            var refreshToken = Convert.ToBase64String(secureRandomBytes);

            return refreshToken;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration _config)
        {
            var secretKey = _config["JWT:SecretKey"] ?? throw new InvalidOperationException("Invalid key");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                                      Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters,
                                                       out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                             !jwtSecurityToken.Header.Alg.Equals(
                             SecurityAlgorithms.HmacSha256,
                             StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }

Etapa 3 ajuste o program
	
		builder.Services.AddScoped<ITokenService, TokenService>();