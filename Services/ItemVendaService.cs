using System;
using System.Globalization;

namespace MeuAppAvalonia;

public sealed class ItemVendaService
{
    public bool TentarLer(
        string? codigo,
        string? nome,
        string? textoPreco,
        string? textoQuantidade,
        out ItemVendaDados dados)
    {
        dados = default;

        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nome) ||
            !decimal.TryParse(textoPreco?.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var preco) || preco < 0 ||
            !int.TryParse(textoQuantidade?.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var quantidade) || quantidade <= 0)
        {
            return false;
        }

        dados = new ItemVendaDados(codigo.Trim(), nome.Trim(), preco, quantidade);
        return true;
    }

    public decimal CalcularTotal(decimal subtotal, string? textoDesconto)
    {
        var desconto = decimal.TryParse(textoDesconto, NumberStyles.Number, CultureInfo.CurrentCulture, out var valor) && valor > 0
            ? valor
            : 0;

        return Math.Max(0, subtotal - desconto);
    }
}

public readonly record struct ItemVendaDados(string Codigo, string Nome, decimal Preco, int Quantidade);
