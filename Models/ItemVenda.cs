using System.Globalization;

namespace MeuAppAvalonia;

public sealed class ItemVenda
{
    public int Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int Quantidade { get; private set; }
    public string TextoPreco => Preco.ToString("C2", CultureInfo.CurrentCulture);
    public decimal Total => Preco * Quantidade;

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
    }
}