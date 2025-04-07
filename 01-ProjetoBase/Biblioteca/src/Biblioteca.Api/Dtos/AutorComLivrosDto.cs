using Biblioteca.Api.Entidades;

namespace Biblioteca.Api.Dtos;

public class AutorComLivrosDto:AutorReadDto
{
    public List<Livro> Livros { get; set; } = [];
}