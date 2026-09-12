using System.Collections.Generic;

namespace MeuAppAvalonia;

public sealed class Venda
{
    public int Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Vendedor { get; set; } = string.Empty;
    public string FormaPagamento { get; set; } = "Dinheiro";
    public decimal Desconto { get; set; }
    public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
}
