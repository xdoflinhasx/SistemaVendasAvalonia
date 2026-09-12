using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeuAppAvalonia.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalErpSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VendaId",
                table: "ItensVenda",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Documento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Vendedor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FormaPagamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Desconto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_VendaId",
                table: "ItensVenda",
                column: "VendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItensVenda_Vendas_VendaId",
                table: "ItensVenda",
                column: "VendaId",
                principalTable: "Vendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensVenda_Vendas_VendaId",
                table: "ItensVenda");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Vendas");

            migrationBuilder.DropTable(
                name: "Vendedores");

            migrationBuilder.DropIndex(
                name: "IX_ItensVenda_VendaId",
                table: "ItensVenda");

            migrationBuilder.DropColumn(
                name: "VendaId",
                table: "ItensVenda");
        }
    }
}
