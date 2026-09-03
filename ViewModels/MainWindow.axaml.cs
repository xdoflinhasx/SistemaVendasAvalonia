using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;

namespace MeuAppAvalonia;

public partial class JanelaPrincipal : Window
{
    public ObservableCollection<ItemVenda> Vendas { get; } = new();

    public bool ListaVazia => Vendas.Count == 0;
    private int _indiceEdicao = -1;
    private readonly DispatcherTimer _temporizadorMensagem = new() { Interval = TimeSpan.FromSeconds(2) };
    private readonly Task<bool> _inicializacaoBanco;

    public JanelaPrincipal()
    {
        InitializeComponent();
        DataContext = this;
        ItensVendaControle.ItemsSource = Vendas;
        Vendas.CollectionChanged += (_, _) => NotificarAlteracao(nameof(ListaVazia));
        _temporizadorMensagem.Tick += (_, _) =>
        {
            MensagemStatus.Text = string.Empty;
            _temporizadorMensagem.Stop();
        };
        _inicializacaoBanco = InicializarBancoAsync();
        AtualizarTotais();
    }

    private async void SalvarBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        var codigo = CodigoProdutoCaixa.Text?.Trim() ?? string.Empty;
        var nome = NomeProdutoCaixa.Text?.Trim() ?? string.Empty;
        var textoPreco = PrecoCaixa.Text?.Trim() ?? string.Empty;
        var textoQuantidade = QuantidadeCaixa.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nome) ||
            !decimal.TryParse(textoPreco, NumberStyles.Number, CultureInfo.CurrentCulture, out var preco) || preco < 0 ||
            !int.TryParse(textoQuantidade, NumberStyles.Integer, CultureInfo.CurrentCulture, out var quantidade) || quantidade <= 0)
        {
            MensagemStatus.Text = "Preencha todos os campos com valores válidos.";
            return;
        }

        if (!await _inicializacaoBanco)
            return;

        await using var banco = new VendasDbContext();
        var indiceEdicao = _indiceEdicao;
        ItemVenda? itemEditado = null;
        if (_indiceEdicao >= 0)
        {
            itemEditado = Vendas[_indiceEdicao];
            itemEditado.Atualizar(codigo, nome, preco, quantidade);
            banco.ItensVenda.Update(itemEditado);
        }
        else
        {
            var item = new ItemVenda(codigo, nome, preco, quantidade);
            Vendas.Add(item);
            await banco.ItensVenda.AddAsync(item);
        }
        await banco.SaveChangesAsync();
        await RecarregarVendasAsync(banco);

        MensagemListaVazia.IsVisible = false;
        MensagemStatus.Text = string.Empty;
        _indiceEdicao = -1;
        SalvarBotao.Content = "Salvar";
        CancelarEdicaoBotao.IsVisible = false;
        LimparFormulario();
        AtualizarTotais();
        CodigoProdutoCaixa.Focus();
    }

    private void EditarBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ItemVenda item })
            return;

        _indiceEdicao = Vendas.IndexOf(item);
        CodigoProdutoCaixa.Text = item.Codigo;
        NomeProdutoCaixa.Text = item.Nome;
        PrecoCaixa.Text = item.Preco.ToString("N2", CultureInfo.CurrentCulture);
        QuantidadeCaixa.Text = item.Quantidade.ToString(CultureInfo.CurrentCulture);
        SalvarBotao.Content = "Atualizar";
        CancelarEdicaoBotao.IsVisible = true;
        MensagemStatus.Text = string.Empty;
        CodigoProdutoCaixa.Focus();
    }

    private async void ExcluirBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ItemVenda item })
            return;

        if (!await _inicializacaoBanco)
            return;

        await using var banco = new VendasDbContext();
        banco.ItensVenda.Remove(item);
        await banco.SaveChangesAsync();
        Vendas.Remove(item);
        MensagemListaVazia.IsVisible = Vendas.Count == 0;
        if (_indiceEdicao >= 0)
            CancelarEdicao();
        MensagemStatus.Text = string.Empty;
        AtualizarTotais();
    }

    private void CancelarEdicaoBotao_Clicado(object? sender, RoutedEventArgs e) => CancelarEdicao();

    private void CancelarEdicao()
    {
        _indiceEdicao = -1;
        SalvarBotao.Content = "Salvar";
        CancelarEdicaoBotao.IsVisible = false;
        LimparFormulario();
        MensagemStatus.Text = string.Empty;
    }

    private void DescontoCaixa_TextoAlterado(object? sender, TextChangedEventArgs e) => AtualizarTotais();

    private void AtualizarTotais()
    {
        var subtotal = Vendas.Sum(item => item.Total);
        var desconto = decimal.TryParse(DescontoCaixa?.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var valor) && valor > 0
            ? valor
            : 0;
        var total = Math.Max(0, subtotal - desconto);
        SubtotalTexto.Text = subtotal.ToString("C2", CultureInfo.CurrentCulture);
        TotalTexto.Text = total.ToString("C2", CultureInfo.CurrentCulture);
    }

    private void LimparFormulario()
    {
        CodigoProdutoCaixa.Text = string.Empty;
        NomeProdutoCaixa.Text = string.Empty;
        PrecoCaixa.Text = string.Empty;
        QuantidadeCaixa.Text = string.Empty;
    }

    private async Task<bool> InicializarBancoAsync()
    {
        try
        {
            await using var banco = new VendasDbContext();
            await PrepararBancoCriadoComEnsureCreatedAsync(banco);
            await banco.Database.MigrateAsync();
            await RecarregarVendasAsync(banco);

            return true;
        }
        catch (Exception exception)
        {
            MensagemStatus.Text = $"Não foi possível conectar ao banco: {exception.Message}";
            return false;
        }
    }

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

                INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                VALUES (N'20260903224328_InitialCreate', N'10.0.11');

                IF COL_LENGTH(N'ItensVenda', N'Atualizado') IS NOT NULL
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES (N'20260903225444_AddAtualizadoToItemVenda', N'10.0.11');
            END
            """;
        await comando.ExecuteNonQueryAsync();
    }

    private async Task RecarregarVendasAsync(VendasDbContext banco)
    {
        var itens = await banco.ItensVenda.AsNoTracking().OrderBy(item => item.Id).ToListAsync();
        Vendas.Clear();
        foreach (var item in itens)
            Vendas.Add(item);
    }

    private void NotificarAlteracao(string nomePropriedade) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nomePropriedade));

    public new event PropertyChangedEventHandler? PropertyChanged;
}

