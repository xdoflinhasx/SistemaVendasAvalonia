using System;

namespace MeuAppAvalonia;

public sealed class ConfiguracaoSecret
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string EmpresaNome { get; set; } = string.Empty;
    public string EmpresaCnpj { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string SecretGerado { get; set; } = string.Empty;
    public string UrlEndpoint { get; set; } = string.Empty;
    public string TokenAcesso { get; set; } = string.Empty;
    public DateTime TokenExpiraEm { get; set; } = DateTime.UtcNow.AddHours(24);
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
}
