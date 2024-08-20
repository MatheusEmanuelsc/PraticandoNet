Realizando teste unitarios  em sua Web Api Com XUnit

apos criar o seu projetono visual studio vc clicar em arquivo depois adicionar 
projetos nisso 

seleciona XUnit Test

apos criar o projeto  
clique no projeto  
depois em adc despencias e adc uma referencia do projeto principal ao de test


Essa Etapa Opcional vc pode  adc a fluentAssertions
Ela não e necessaria para utilizar o xunit mais nos exemplos abaixo como uma forma de  mostra que vc pode utilizar algumas coisas ara auxiliar nos teste vamos utilizar tbm


etapa 1 criei um controlador para facilitar a reutilização de codigo exemplo


 public class AlunosUnitTestController
    {
        public IUnitOfWork repository;
        public IMapper mapper;
        public static DbContextOptions<AppDbContext> dbContextOptions { get; }
        public static string connectionString =
          "Server=localhost;DataBase=apicatalogodb;Uid=root;Pwd=b1b2b3b4";
        static AlunosUnitTestController()
        {
            dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
               .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
               .Options;
        }
        public AlunosUnitTestController()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });

            mapper = config.CreateMapper();

            var context = new AppDbContext(dbContextOptions);
            repository = new UnitOfWork(context);
        }
    }


   Etapa 2  agora criei a classe com o metodo que vc deseja testa Exemplo

   public class GetAlunosUnitTest : IClassFixture<AlunosUnitTestController>
    {
        private readonly AlunosController _controller;

        public GetAlunosUnitTest(AlunosUnitTestController controller)
        {
            _controller = new AlunosController(controller.repository, controller.mapper);
        }
    }

        depois so criar o metodo


    Etapa 3  criar o metodo de teste

        o metodo que vc deseja Testa para ele ser reconhecido pelo xunit Como o caso de teste
        Precisa do atributo [Fact] em cima dele

        O metodo e divido em 3 parte assange act e assert chat gpt explique como funciona eles

        Exemplo de metodo

             [Fact]
        public async Task GetAluno_OkResult()
        {
            //Arrange
            var aluno = 2;

            //Act
            var data = await _controller.GetAluno(aluno);
            
           //(xunit ) 
           // var okResult = Assert.IsType<OkObjectResult>(data.Result);
           // Assert.Equal(200,okResult.StatusCode);

            // Assert(Fluentassertions)
            data.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
        }


        exemplo de pilha de teste unitarios


        public class GetAlunosUnitTest : IClassFixture<AlunosUnitTestController>
    {
        private readonly AlunosController _controller;

        public GetAlunosUnitTest(AlunosUnitTestController controller)
        {
            _controller = new AlunosController(controller.repository, controller.mapper);
        }

        [Fact]
        public async Task GetAluno_OkResult()
        {
            //Arrange
            var aluno = 2;

            //Act
            var data = await _controller.GetAluno(aluno);
            
           //(xunit ) 
            var okResult = Assert.IsType<OkObjectResult>(data.Result);
            Assert.Equal(200,okResult.StatusCode);

            // Assert(Fluentassertions)
           // data.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
        }


        [Fact]
        public async Task GetAluno_Return_NotFound()
        {
            // Arrange
            var aluno = 999;

            // Act
            var data = await _controller.GetAluno(aluno);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(data.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetAluno_Return_BadRequest()
        {
            // Arrange
            var aluno = -1;

            // Act
            var data = await _controller.GetAluno(aluno);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestResult>(data.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

    
    }



    Chatgp4 expluqe  pq tem resultados testadndo com codigos object  e outros apenas o codigo + result
    