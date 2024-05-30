using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebApplication1.Models;

    public class Produto
    {
        [Key]
        [Required]
        public int ProdutoId { get; set; }
        [Required]
        [StringLength(80)]
        public string? Nome { get; set; }
        [Required]
        [StringLength(300)]
        public string? Desc { get; set; }
        [Required]
        [Column(TypeName ="decimal(10.2)")]
        public decimal Preco { get; set; }
        [Required]
        [StringLength(300)]
        public string? ImgUrl { get; set; }
        public double Estoque { get; set; }
        public DateTime DataCadastro { get; set; }

    // E opcional mais vc pode explicita ainda mais o Relacionamento
        [JsonIgnore]
        public int CategoriaId { get; set; }
        [JsonIgnore]
        public Categoria? Categoria { get; set; }
         }

