using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalogo.Migrations
{
    /// <inheritdoc />
    public partial class PopularProdutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            mb.Sql("Insert into Produtos(Nome,Descricao,Preco,ImagemUrl,Estoque,Datacadastro,CategoriaId) "+
                "Values('Coca-Cola Diet,'Refrigerante Cola 350ml',5.45,'cocacola.jpg',50,now(),1");
            mb.Sql("Insert into Produtos(Nome,Descricao,Preco,ImagemUrl,Estoque,Datacadastro,CategoriaId) " +
               "Values('Lanche Atum ,'atum duplo',10.50,'atum.jpg',50,now(),2");
            mb.Sql("Insert into Produtos(Nome,Descricao,Preco,ImagemUrl,Estoque,Datacadastro,CategoriaId) " +
               "Values('Sorvertao,'chocolate',7.50,'sorverte.jpg',50,now(),3");
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder mb)
        {
            mb.Sql("Delete from Produtos");
        }
    }
}
