namespace MeuAppAvalonia;

public sealed class Produto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
}
