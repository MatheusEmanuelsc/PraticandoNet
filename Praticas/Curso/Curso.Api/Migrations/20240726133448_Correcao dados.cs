using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Curso.Api.Migrations
{
    /// <inheritdoc />
    public partial class Correcaodados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Nota",
                table: "Alunos",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Nota",
                table: "Alunos",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");
        }
    }
}
