using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers;

[Route("[controller]")]
[ApiController]
public class ProdutosController : ControllerBase
{
    private readonly IRepository<Produto> _repository;
    private readonly IProdutoRepository _produtosRepository;
    public ProdutosController(IRepository<Produto> repository, IProdutoRepository produtosRepository)
    {

        _repository = repository;
        _produtosRepository = produtosRepository;
    }

   [HttpGet("produtos/{id}")]
   public ActionResult<IEnumerable<Produto>> GetProdutosCategoria(int id)
    {
        var produtos = _produtosRepository.GetProdutosPorCategoria(id);
        if (produtos is null)
        {
            return NotFound();
        }
        return Ok(produtos);
    }
   [HttpGet]
    public ActionResult<IEnumerable<Produto>> Get()
    {
        var produtos =  _repository.GetAll();
        if (produtos is null)
        {
            return NotFound();
        }
        return produtos;
    }

    [HttpGet("{id}", Name = "ObterProduto")]
    public ActionResult<Produto> Get(int id)
    {
        var produto = _repository.GetProduto(id);
        if (produto is null)
        {
            return NotFound("Produto não encontrado...");
        }
        return Ok(produto);
    }

    [HttpPost]
    public ActionResult Post(Produto produto)
    {
        if (produto is null)
            return BadRequest();

        var novoProduto=_repository.Create(produto);

        return new CreatedAtRouteResult("ObterProduto",
            new { id = novoProduto.ProdutoId }, novoProduto);
    }

    [HttpPut("{id:int}")]
    public ActionResult Put(int id, Produto produto)
    {
        if (id != produto.ProdutoId)
        {
            return BadRequest();
        }

        var atualizado = _repository.Update(produto);

        
        if (atualizado)
        {
            return Ok(produto);
        }
        else
        {
            return StatusCode(500,$"Falha ao atualizar o produto de id = {id}");
        }
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var deletado = _repository.GetProduto(id);
        if (deletado == null)
        {
            
            return NotFound($" Produto com id={id} não encontrada...");
        }

        var produtoExcluida = _repository.Delete(id);
        return Ok(produtoExcluida);
    }
}