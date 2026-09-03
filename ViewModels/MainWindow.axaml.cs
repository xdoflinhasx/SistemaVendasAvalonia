using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace MeuAppAvalonia;

public partial class JanelaPrincipal : Window
{
    public ObservableCollection<ItemVenda> Vendas { get; } = new();

    public bool ListaVazia => Vendas.Count == 0;
    private int _indiceEdicao = -1;
    private readonly DispatcherTimer _temporizadorMensagem = new() { Interval = TimeSpan.FromSeconds(2) };

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
        AtualizarTotais();
    }

    private void SalvarBotao_Clicado(object? sender, RoutedEventArgs e)
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

        var item = new ItemVenda(codigo, nome, preco, quantidade);
        if (_indiceEdicao >= 0)
        {
            Vendas[_indiceEdicao] = item;
        }
        else
        {
            Vendas.Add(item);
        }

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

    private void ExcluirBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ItemVenda item })
            return;

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

    private void NotificarAlteracao(string nomePropriedade) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nomePropriedade));

    public new event PropertyChangedEventHandler? PropertyChanged;
}

