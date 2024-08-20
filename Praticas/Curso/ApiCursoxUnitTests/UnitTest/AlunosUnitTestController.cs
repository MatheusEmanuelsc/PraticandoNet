using AutoMapper;
using Curso.Api.Context;
using Curso.Api.Dtos;
using Curso.Api.Profiles;
using Curso.Api.Repositorys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCursoxUnitTests.UnitTest
{
    public class AlunosUnitTestController
    {
        public IUnitOfWork repository;
        public IMapper mapper;
        public static DbContextOptions<AppDbContext> dbContextOptions { get; }
        public static string connectionString =
          "Server=localhost;DataBase=DbCurso;Uid=root;Pwd=b1b2b3b4";
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
}
