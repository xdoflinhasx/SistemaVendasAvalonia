using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;

namespace MeuAppAvalonia;

public sealed class VendaEmMemoria
{
    public string Cliente { get; set; } = string.Empty;
    public string Vendedor { get; set; } = string.Empty;
    public string FormaPagamento { get; set; } = "Dinheiro";
    public decimal Desconto { get; set; }
    public ObservableCollection<ItemVenda> Itens { get; } = new();
}

public partial class JanelaPrincipal : Window
{
    public ObservableCollection<ItemVenda> Vendas { get; } = new();
    public ObservableCollection<Cliente> ClientesDisponiveis { get; } = new();
    public ObservableCollection<Vendedor> VendedoresDisponiveis { get; } = new();
    public ObservableCollection<Produto> ProdutosDisponiveis { get; } = new();
    public ObservableCollection<string> FormasPagamentoDisponiveis { get; } = new();

    public bool ListaVazia => Vendas.Count == 0;

    private readonly List<VendaEmMemoria> _historicoVendas = new();
    private readonly ItemVendaService _itemVendaService = new();
    private readonly VendasService _vendasService = new();
    private int _indiceRegistroAtual = -1;
    private int _indiceEdicaoItem = -1;

    public JanelaPrincipal()
    {
        InitializeComponent();
        DataContext = this;
        ItensVendaControle.ItemsSource = Vendas;

        FormasPagamentoDisponiveis.Add("Dinheiro");
        FormasPagamentoDisponiveis.Add("PIX");
        FormasPagamentoDisponiveis.Add("Cartão de Crédito");
        FormasPagamentoDisponiveis.Add("Cartão de Débito");
        FormasPagamentoDisponiveis.Add("Boleto");

        ClienteCaixa.ItemsSource = ClientesDisponiveis;
        VendedorCaixa.ItemsSource = VendedoresDisponiveis;
        CodigoProdutoCaixa.ItemsSource = ProdutosDisponiveis;
        FormaPagamentoComboBox.ItemsSource = FormasPagamentoDisponiveis;
        FormaPagamentoComboBox.SelectedIndex = 0;

        _ = CarregarDadosBaseAsync();

        Vendas.CollectionChanged += (_, _) =>
        {
            MensagemListaVazia.IsVisible = Vendas.Count == 0;
            AtualizarTotais();
            NotificarAlteracao(nameof(ListaVazia));
        };

        CriarNovaVendaEmMemoria();
        AtualizarTotais();
    }

    private void NovaVendaBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        CriarNovaVendaEmMemoria();
        MensagemStatus.Text = "Nova venda criada em memória.";
    }

    private void AlterarBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        if (VendaAtual is null)
        {
            MensagemStatus.Text = "Nenhuma venda em edição.";
            return;
        }

        MensagemStatus.Text = "Venda selecionada para alteração local.";
    }

    private void CancelarVendaBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        if (VendaAtual is null)
        {
            MensagemStatus.Text = "Não há venda ativa para cancelar.";
            return;
        }

        ClienteCaixa.Text = string.Empty;
        VendedorCaixa.Text = string.Empty;
        FormaPagamentoComboBox.SelectedIndex = 0;
        DescontoCaixa.Text = string.Empty;
        Vendas.Clear();
        VendaAtual.Cliente = string.Empty;
        VendaAtual.Vendedor = string.Empty;
        VendaAtual.FormaPagamento = "Dinheiro";
        VendaAtual.Desconto = 0m;
        VendaAtual.Itens.Clear();
        LimparFormularioItem();
        MensagemStatus.Text = "Venda cancelada. O estado da tela foi limpo.";
        AtualizarTotais();
    }

    private void VoltarRegistroBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        if (_historicoVendas.Count == 0 || _indiceRegistroAtual <= 0)
            return;

        _indiceRegistroAtual--;
        CarregarVendaAtualNaTela();
        MensagemStatus.Text = "Registro anterior carregado.";
    }

    private void AvancarRegistroBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        if (_historicoVendas.Count == 0 || _indiceRegistroAtual >= _historicoVendas.Count - 1)
            return;

        _indiceRegistroAtual++;
        CarregarVendaAtualNaTela();
        MensagemStatus.Text = "Próximo registro carregado.";
    }

    private async void SalvarVendaBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        var venda = VendaAtual ?? CriarNovaVendaEmMemoria();

        venda.Cliente = (ClienteCaixa.Text ?? string.Empty).Trim();
        venda.Vendedor = (VendedorCaixa.Text ?? string.Empty).Trim();
        venda.FormaPagamento = FormaPagamentoComboBox.SelectedItem as string ?? "Dinheiro";
        venda.Desconto = decimal.TryParse(DescontoCaixa.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out var desconto)
            ? desconto
            : 0m;
        venda.Itens.Clear();
        foreach (var item in Vendas)
            venda.Itens.Add(item);

        await PersistirVendaAsync(venda);

        MensagemStatus.Text = "Venda salva com sucesso no banco de dados.";
    }

    private void CodigoProdutoCaixa_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CodigoProdutoCaixa.SelectedItem is not Produto produtoSelecionado)
            return;

        NomeProdutoCaixa.Text = produtoSelecionado.Nome;
        PrecoCaixa.Text = produtoSelecionado.Preco.ToString("N2", CultureInfo.CurrentCulture);
        QuantidadeCaixa.Text = "1";
        MensagemStatus.Text = $"Produto selecionado: {produtoSelecionado.Codigo} - {produtoSelecionado.Nome}. Ajuste o preço e a quantidade antes de adicionar.";
    }

    private void SalvarBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        var codigoProduto = (CodigoProdutoCaixa.SelectedItem as Produto)?.Codigo ?? string.Empty;

        if (!_itemVendaService.TentarLer(
            codigoProduto,
            NomeProdutoCaixa.Text ?? string.Empty,
            PrecoCaixa.Text ?? string.Empty,
            QuantidadeCaixa.Text ?? string.Empty,
            out var dados))
        {
            MensagemStatus.Text = "Preencha todos os campos de item com valores válidos.";
            return;
        }

        if (_indiceEdicaoItem >= 0 && _indiceEdicaoItem < Vendas.Count)
        {
            Vendas[_indiceEdicaoItem].Atualizar(dados.Codigo, dados.Nome, dados.Preco, dados.Quantidade);
        }
        else
        {
            Vendas.Add(new ItemVenda(dados.Codigo, dados.Nome, dados.Preco, dados.Quantidade));
        }

        if (VendaAtual is not null)
        {
            VendaAtual.Itens.Clear();
            foreach (var item in Vendas)
                VendaAtual.Itens.Add(item);
        }

        MensagemStatus.Text = "Item adicionado/atualizado em memória.";
        _indiceEdicaoItem = -1;
        SalvarBotao.Content = "Adicionar Item";
        CancelarEdicaoBotao.IsVisible = false;
        LimparFormularioItem();
        AtualizarTotais();
    }

    private void EditarBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ItemVenda item })
            return;

        _indiceEdicaoItem = Vendas.IndexOf(item);
        var produtoAtual = ProdutosDisponiveis.FirstOrDefault(produto => produto.Codigo == item.Codigo);
        CodigoProdutoCaixa.SelectedItem = produtoAtual;
        NomeProdutoCaixa.Text = item.Nome;
        PrecoCaixa.Text = item.Preco.ToString("N2", CultureInfo.CurrentCulture);
        QuantidadeCaixa.Text = item.Quantidade.ToString(CultureInfo.CurrentCulture);
        SalvarBotao.Content = "Atualizar Item";
        CancelarEdicaoBotao.IsVisible = true;
        MensagemStatus.Text = string.Empty;
        CodigoProdutoCaixa.Focus();
    }

    private void ExcluirBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ItemVenda item })
            return;

        Vendas.Remove(item);
        if (VendaAtual is not null)
        {
            VendaAtual.Itens.Clear();
            foreach (var itemVenda in Vendas)
                VendaAtual.Itens.Add(itemVenda);
        }

        if (_indiceEdicaoItem >= 0)
            CancelarEdicao();

        MensagemStatus.Text = string.Empty;
        AtualizarTotais();
    }

    private void CancelarEdicaoBotao_Clicado(object? sender, RoutedEventArgs e) => CancelarEdicao();

    private void ConfigurarSecretBotao_Clicado(object? sender, RoutedEventArgs e)
    {
        var janelaConfiguracao = new ConfiguracaoSecretWindow();
        janelaConfiguracao.Show(this);
    }

    private void CancelarEdicao()
    {
        _indiceEdicaoItem = -1;
        SalvarBotao.Content = "Adicionar Item";
        CancelarEdicaoBotao.IsVisible = false;
        LimparFormularioItem();
        MensagemStatus.Text = string.Empty;
    }

    private void DescontoCaixa_TextoAlterado(object? sender, TextChangedEventArgs e)
    {
        if (VendaAtual is not null)
        {
            VendaAtual.Desconto = decimal.TryParse(DescontoCaixa.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out var desconto)
                ? desconto
                : 0m;
        }

        AtualizarTotais();
    }

    private void AtualizarTotais()
    {
        var subtotal = Vendas.Sum(item => item.Total);
        var total = _itemVendaService.CalcularTotal(subtotal, DescontoCaixa?.Text);
        SubtotalTexto.Text = subtotal.ToString("C2", CultureInfo.CurrentCulture);
        TotalTexto.Text = total.ToString("C2", CultureInfo.CurrentCulture);
    }

    private void LimparFormularioItem()
    {
        CodigoProdutoCaixa.SelectedIndex = -1;
        NomeProdutoCaixa.Text = string.Empty;
        PrecoCaixa.Text = string.Empty;
        QuantidadeCaixa.Text = string.Empty;
    }

    private async Task CarregarDadosBaseAsync()
    {
        try
        {
            await using var banco = new VendasDbContext();

            var clientes = await banco.Clientes.AsNoTracking().OrderBy(cliente => cliente.Nome).ToListAsync();
            var vendedores = await banco.Vendedores.AsNoTracking().OrderBy(vendedor => vendedor.Nome).ToListAsync();
            var produtos = await banco.Produtos.AsNoTracking().OrderBy(produto => produto.Nome).ToListAsync();

            ClientesDisponiveis.Clear();
            foreach (var cliente in clientes)
                ClientesDisponiveis.Add(cliente);

            VendedoresDisponiveis.Clear();
            foreach (var vendedor in vendedores)
                VendedoresDisponiveis.Add(vendedor);

            ProdutosDisponiveis.Clear();
            foreach (var produto in produtos)
                ProdutosDisponiveis.Add(produto);

            if (ClientesDisponiveis.Count > 0)
                ClienteCaixa.SelectedItem = ClientesDisponiveis[0];

            if (VendedoresDisponiveis.Count > 0)
                VendedorCaixa.SelectedItem = VendedoresDisponiveis[0];

            if (ProdutosDisponiveis.Count > 0)
                CodigoProdutoCaixa.SelectedIndex = 0;
        }
        catch
        {
            MensagemStatus.Text = "Não foi possível carregar os dados base do banco. A tela continuará com os valores locais.";
        }
    }

    private VendaEmMemoria? VendaAtual =>
        _indiceRegistroAtual >= 0 && _indiceRegistroAtual < _historicoVendas.Count
            ? _historicoVendas[_indiceRegistroAtual]
            : null;

    private VendaEmMemoria CriarNovaVendaEmMemoria()
    {
        var venda = new VendaEmMemoria
        {
            FormaPagamento = "Dinheiro"
        };

        _historicoVendas.Add(venda);
        _indiceRegistroAtual = _historicoVendas.Count - 1;
        CarregarVendaAtualNaTela();
        return venda;
    }

    private void CarregarVendaAtualNaTela()
    {
        var venda = VendaAtual;
        if (venda is null)
            return;

        ClienteCaixa.Text = venda.Cliente;
        VendedorCaixa.Text = venda.Vendedor;
        FormaPagamentoComboBox.SelectedItem = venda.FormaPagamento;
        DescontoCaixa.Text = venda.Desconto.ToString("N2", CultureInfo.CurrentCulture);

        Vendas.Clear();
        foreach (var item in venda.Itens)
            Vendas.Add(item);

        MensagemListaVazia.IsVisible = Vendas.Count == 0;
        AtualizarTotais();
    }

    private async Task PersistirVendaAsync(VendaEmMemoria venda)
    {
        if (venda.Itens.Count == 0)
        {
            MensagemStatus.Text = "Não é possível salvar uma venda sem itens.";
            return;
        }

        try
        {
            var vendaEntity = new Venda
            {
                Id = 0,
                Cliente = venda.Cliente,
                Vendedor = venda.Vendedor,
                FormaPagamento = venda.FormaPagamento,
                Desconto = venda.Desconto,
                Itens = venda.Itens.ToList()
            };

            var itensPersistidos = await _vendasService.SalvarVendaAsync(vendaEntity, venda.Itens.ToList());
            Vendas.Clear();
            foreach (var item in itensPersistidos)
                Vendas.Add(item);

            venda.Itens.Clear();
            foreach (var item in Vendas)
                venda.Itens.Add(item);
        }
        catch (Exception exception)
        {
            MensagemStatus.Text = $"Não foi possível salvar a venda: {exception.Message}";
        }
    }

    private void NotificarAlteracao(string nomePropriedade) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nomePropriedade));

    public new event PropertyChangedEventHandler? PropertyChanged;
}

