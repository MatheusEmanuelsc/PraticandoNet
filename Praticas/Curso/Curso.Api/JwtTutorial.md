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

