using Curso.Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCursoxUnitTests.UnitTest
{
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
}
