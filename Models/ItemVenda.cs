using System.Globalization;

namespace MeuAppAvalonia;

public sealed class ItemVenda
{
    public string Codigo { get; }
    public string Nome { get; }
    public string TextoPreco { get; }
    public decimal Preco { get; }
    public int Quantidade { get; }
    public decimal Total => Preco * Quantidade;

    public ItemVenda(string codigo, string nome, decimal preco, int quantidade)
    {
        Codigo = codigo;
        Nome = nome;
        Preco = preco;
        TextoPreco = preco.ToString("C2", CultureInfo.CurrentCulture);
        Quantidade = quantidade;
    }
}