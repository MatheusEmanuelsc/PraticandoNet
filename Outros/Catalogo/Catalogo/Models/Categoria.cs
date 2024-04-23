using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Catalogo.Context;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Models
{
    public class Categoria
    {
        [Key]
        public int CategoriaId { get; set; }
        [Required]
        [StringLength(80)]
        public string? Nome { get; set; }
        [Required]
        [StringLength(300)]
        public string? ImagemUrl { get; }

        public Categoria()
        {
            //E uma boa pratica inicializar essa coleção
            Produtos = new List<Produto>();
        }
        [JsonIgnore]
        public ICollection<Produto> Produtos { get; set; }

    }

}
