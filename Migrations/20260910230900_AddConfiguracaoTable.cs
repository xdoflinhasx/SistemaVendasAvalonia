using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeuAppAvalonia.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracaoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuracao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmpresaNome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmpresaCnpj = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SecretGerado = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    UrlEndpoint = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracao", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuracao");
        }
    }
}
