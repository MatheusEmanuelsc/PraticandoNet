

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplication1.Models;

    public class Categoria
    {

        [Key]
        [Required]
        public int CategoriaId { get; set; }
        [Required]
        [StringLength(80)]
        public string? Nome { get; set; }
        [Required]
        [StringLength(300)]
        public string? ImgUrl { get; set; }

        public Categoria()
        {
            // E uma boa pratica que a prpria classe inicie a coleção
            Produtos = new Collection<Produto>();
        }

        // Define o Relacionamento 1 para muitos
        [JsonIgnore]
        public ICollection<Produto>? Produtos { get; set; }  
}

