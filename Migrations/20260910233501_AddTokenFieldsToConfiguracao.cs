using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeuAppAvalonia.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenFieldsToConfiguracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'Configuracao', N'TokenAcesso') IS NULL
                    ALTER TABLE [Configuracao] ADD [TokenAcesso] nvarchar(500) NOT NULL DEFAULT '';

                IF COL_LENGTH(N'Configuracao', N'TokenExpiraEm') IS NULL
                    ALTER TABLE [Configuracao] ADD [TokenExpiraEm] datetime2 NOT NULL DEFAULT SYSUTCDATETIME();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'Configuracao', N'TokenAcesso') IS NOT NULL
                    ALTER TABLE [Configuracao] DROP COLUMN [TokenAcesso];

                IF COL_LENGTH(N'Configuracao', N'TokenExpiraEm') IS NOT NULL
                    ALTER TABLE [Configuracao] DROP COLUMN [TokenExpiraEm];
                """);
        }
    }
}
