using System.ComponentModel;
using System.Globalization;

namespace MeuAppAvalonia;

public sealed class ItemVenda : INotifyPropertyChanged
{
    public int Id { get; private set; }
    public int? VendaId { get; set; }
    public Venda? Venda { get; set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int Quantidade { get; private set; }
    public bool Atualizado { get; private set; }
    public string TextoPreco => Preco.ToString("C2", CultureInfo.CurrentCulture);
    public decimal Total => Preco * Quantidade;

    public event PropertyChangedEventHandler? PropertyChanged;

    private ItemVenda()
    {
    }

    public ItemVenda(string codigo, string nome, decimal preco, int quantidade)
    {
        Codigo = codigo;
        Nome = nome;
        Preco = preco;
        Quantidade = quantidade;
    }

    public void Atualizar(string codigo, string nome, decimal preco, int quantidade)
    {
        Codigo = codigo;
        Nome = nome;
        Preco = preco;
        Quantidade = quantidade;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Codigo)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nome)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Preco)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantidade)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoPreco)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Total)));
    }
}