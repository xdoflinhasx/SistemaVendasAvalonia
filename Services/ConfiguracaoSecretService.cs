using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MeuAppAvalonia;

public sealed class ConfiguracaoSecretService
{
    public async Task<ConfiguracaoSecret?> CarregarAsync()
    {
        await using var banco = new VendasDbContext();
        await GarantirTabelaConfiguracaoAsync(banco);

        return await banco.ConfiguracoesSecret
            .AsNoTracking()
            .OrderByDescending(item => item.Id)
            .FirstOrDefaultAsync();
    }

    public async Task SalvarAsync(
        string clientId,
        string empresaNome,
        string empresaCnpj,
        string deviceName,
        string secretGerado,
        string urlEndpoint,
        string tokenAcesso,
        DateTime? tokenExpiraEm = null)
    {
        await using var banco = new VendasDbContext();
        await GarantirTabelaConfiguracaoAsync(banco);

        var configuracao = await banco.ConfiguracoesSecret
            .OrderByDescending(item => item.Id)
            .FirstOrDefaultAsync();

        if (configuracao is null)
        {
            configuracao = new ConfiguracaoSecret();
            banco.ConfiguracoesSecret.Add(configuracao);
        }

        configuracao.ClientId = clientId.Trim();
        configuracao.EmpresaNome = empresaNome.Trim();
        configuracao.EmpresaCnpj = empresaCnpj.Trim();
        configuracao.DeviceName = deviceName.Trim();
        configuracao.SecretGerado = secretGerado.Trim();
        configuracao.UrlEndpoint = urlEndpoint.Trim();
        configuracao.TokenAcesso = tokenAcesso.Trim();
        configuracao.TokenExpiraEm = tokenExpiraEm ?? DateTime.UtcNow.AddHours(24);
        configuracao.DataAtualizacao = DateTime.Now;

        await banco.SaveChangesAsync();
    }

    private static async Task GarantirTabelaConfiguracaoAsync(VendasDbContext banco)
    {
        var sql = """
            IF OBJECT_ID(N'Configuracao', N'U') IS NULL
            BEGIN
                CREATE TABLE [Configuracao]
                (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [ClientId] nvarchar(200) NOT NULL,
                    [EmpresaNome] nvarchar(200) NOT NULL,
                    [EmpresaCnpj] nvarchar(30) NOT NULL,
                    [DeviceName] nvarchar(200) NOT NULL,
                    [SecretGerado] nvarchar(300) NOT NULL,
                    [UrlEndpoint] nvarchar(1000) NOT NULL,
                    [TokenAcesso] nvarchar(500) NOT NULL DEFAULT '',
                    [TokenExpiraEm] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
                    [DataAtualizacao] datetime2 NOT NULL,
                    CONSTRAINT [PK_Configuracao] PRIMARY KEY ([Id])
                );
            END
            ELSE
            BEGIN
                IF COL_LENGTH(N'Configuracao', N'TokenAcesso') IS NULL
                    ALTER TABLE [Configuracao] ADD [TokenAcesso] nvarchar(500) NOT NULL DEFAULT '';

                IF COL_LENGTH(N'Configuracao', N'TokenExpiraEm') IS NULL
                    ALTER TABLE [Configuracao] ADD [TokenExpiraEm] datetime2 NOT NULL DEFAULT SYSUTCDATETIME();
            END
            """;

        await banco.Database.ExecuteSqlRawAsync(sql);
    }
}
