using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MeuAppAvalonia;

public sealed class VendasService
{
    public async Task<IReadOnlyList<ItemVenda>> InicializarAsync()
    {
        await using var banco = new VendasDbContext();
        await PrepararBancoCriadoComEnsureCreatedAsync(banco);
        await banco.Database.MigrateAsync();
        return await ListarAsync(banco);
    }

    public async Task<IReadOnlyList<ItemVenda>> SalvarAsync(
        ItemVenda? itemExistente,
        string codigo,
        string nome,
        decimal preco,
        int quantidade)
    {
        await using var banco = new VendasDbContext();

        if (itemExistente is null)
        {
            await banco.ItensVenda.AddAsync(new ItemVenda(codigo, nome, preco, quantidade));
        }
        else
        {
            itemExistente.Atualizar(codigo, nome, preco, quantidade);
            banco.ItensVenda.Update(itemExistente);
        }

        await banco.SaveChangesAsync();
        return await ListarAsync(banco);
    }

    public async Task<IReadOnlyList<ItemVenda>> SalvarVendaAsync(Venda venda, IReadOnlyCollection<ItemVenda> itens)
    {
        await using var banco = new VendasDbContext();

        var vendaExistente = await banco.Vendas
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == venda.Id);

        if (vendaExistente is null)
        {
            var novaVenda = new Venda
            {
                Cliente = venda.Cliente,
                Vendedor = venda.Vendedor,
                FormaPagamento = venda.FormaPagamento,
                Desconto = venda.Desconto,
                Itens = new List<ItemVenda>()
            };

            foreach (var item in itens)
            {
                var itemPersistido = new ItemVenda(item.Codigo, item.Nome, item.Preco, item.Quantidade)
                {
                    Venda = novaVenda,
                    VendaId = null
                };
                novaVenda.Itens.Add(itemPersistido);
            }

            await banco.Vendas.AddAsync(novaVenda);
        }
        else
        {
            vendaExistente.Cliente = venda.Cliente;
            vendaExistente.Vendedor = venda.Vendedor;
            vendaExistente.FormaPagamento = venda.FormaPagamento;
            vendaExistente.Desconto = venda.Desconto;

            banco.ItensVenda.RemoveRange(vendaExistente.Itens);
            vendaExistente.Itens = new List<ItemVenda>();

            foreach (var item in itens)
            {
                var itemPersistido = new ItemVenda(item.Codigo, item.Nome, item.Preco, item.Quantidade)
                {
                    Venda = vendaExistente,
                    VendaId = vendaExistente.Id
                };
                vendaExistente.Itens.Add(itemPersistido);
            }

            banco.Vendas.Update(vendaExistente);
        }

        await banco.SaveChangesAsync();
        return await ListarAsync(banco);
    }

    public async Task ExcluirAsync(ItemVenda item)
    {
        await using var banco = new VendasDbContext();
        banco.ItensVenda.Remove(item);
        await banco.SaveChangesAsync();
    }

    private static async Task<IReadOnlyList<ItemVenda>> ListarAsync(VendasDbContext banco) =>
        await banco.ItensVenda.AsNoTracking().OrderBy(item => item.Id).ToListAsync();

    private static async Task PrepararBancoCriadoComEnsureCreatedAsync(VendasDbContext banco)
    {
        var conexao = banco.Database.GetDbConnection();
        await banco.Database.OpenConnectionAsync();

        await using var comando = conexao.CreateCommand();
        comando.CommandText = """
                IF OBJECT_ID(N'ItensVenda', N'U') IS NOT NULL
                    AND OBJECT_ID(N'__EFMigrationsHistory', N'U') IS NULL
            BEGIN
                CREATE TABLE [__EFMigrationsHistory]
                (
                    [MigrationId] nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                );

            END

            IF OBJECT_ID(N'ItensVenda', N'U') IS NOT NULL
               AND OBJECT_ID(N'__EFMigrationsHistory', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory])
            BEGIN
                INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                VALUES (N'20260903224328_InitialCreate', N'10.0.11');

                IF COL_LENGTH(N'ItensVenda', N'Atualizado') IS NOT NULL
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES (N'20260903225444_AddAtualizadoToItemVenda', N'10.0.11');
            END

            IF OBJECT_ID(N'Configuracao', N'U') IS NOT NULL
               AND OBJECT_ID(N'__EFMigrationsHistory', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260910230900_AddConfiguracaoTable')
            BEGIN
                INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                VALUES (N'20260910230900_AddConfiguracaoTable', N'10.0.11');
            END

            IF OBJECT_ID(N'Configuracao', N'U') IS NOT NULL
               AND OBJECT_ID(N'__EFMigrationsHistory', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260910233501_AddTokenFieldsToConfiguracao')
            BEGIN
                INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                VALUES (N'20260910233501_AddTokenFieldsToConfiguracao', N'10.0.11');
            END

            IF OBJECT_ID(N'Configuracao', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'Configuracao', N'TokenAcesso') IS NULL
                    ALTER TABLE [Configuracao] ADD [TokenAcesso] nvarchar(500) NOT NULL DEFAULT '';

                IF COL_LENGTH(N'Configuracao', N'TokenExpiraEm') IS NULL
                    ALTER TABLE [Configuracao] ADD [TokenExpiraEm] datetime2 NOT NULL DEFAULT SYSUTCDATETIME();
            END

            IF OBJECT_ID(N'Clientes', N'U') IS NULL
            BEGIN
                CREATE TABLE [Clientes]
                (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [Nome] nvarchar(200) NOT NULL,
                    [Documento] nvarchar(50) NOT NULL,
                    CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
                );
            END

            IF OBJECT_ID(N'Vendedores', N'U') IS NULL
            BEGIN
                CREATE TABLE [Vendedores]
                (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [Nome] nvarchar(200) NOT NULL,
                    [Codigo] nvarchar(50) NOT NULL,
                    CONSTRAINT [PK_Vendedores] PRIMARY KEY ([Id])
                );
            END

            IF OBJECT_ID(N'Produtos', N'U') IS NULL
            BEGIN
                CREATE TABLE [Produtos]
                (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [Codigo] nvarchar(50) NOT NULL,
                    [Nome] nvarchar(200) NOT NULL,
                    [Preco] decimal(18,2) NOT NULL,
                    CONSTRAINT [PK_Produtos] PRIMARY KEY ([Id])
                );
            END

            IF NOT EXISTS (SELECT 1 FROM [Clientes])
            BEGIN
                INSERT INTO [Clientes] ([Nome], [Documento])
                VALUES
                    (N'João da Silva', N'123.456.789-00'),
                    (N'Maria Oliveira', N'987.654.321-00'),
                    (N'Carlos Souza', N'111.222.333-44');
            END

            IF NOT EXISTS (SELECT 1 FROM [Vendedores])
            BEGIN
                INSERT INTO [Vendedores] ([Nome], [Codigo])
                VALUES
                    (N'Pedro Almeida', N'VEN-001'),
                    (N'Ana Pereira', N'VEN-002'),
                    (N'Lucas Costa', N'VEN-003');
            END

            IF NOT EXISTS (SELECT 1 FROM [Produtos])
            BEGIN
                INSERT INTO [Produtos] ([Codigo], [Nome], [Preco])
                VALUES
                    (N'001', N'Teclado Mecânico', 249.90),
                    (N'002', N'Mouse Gamer', 189.90),
                    (N'003', N'Monitor 24"', 899.00),
                    (N'004', N'Headset Wireless', 479.90),
                    (N'005', N'Webcam Full HD', 329.90);
            END
            """;
        await comando.ExecuteNonQueryAsync();
    }
}
