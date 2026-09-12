using System;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WinFormsClipboard = System.Windows.Forms.Clipboard;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace MeuAppAvalonia;

public partial class ConfiguracaoSecretWindow : Window
{
    private const string EndpointBase = "https://softcomshop.meusoftcom.com.br/softauth/device/add";
    private readonly ConfiguracaoSecretService _configuracaoSecretService = new();

    public ConfiguracaoSecretWindow()
    {
        InitializeComponent();
        _ = CarregarConfiguracaoAsync();
    }

    private async Task CarregarConfiguracaoAsync()
    {
        var configuracao = await _configuracaoSecretService.CarregarAsync();

        if (configuracao is null)
        {
            ClientIdTextBox.Text = "2IJrLCt9JJWU-TVCKbcpNVBmbmkVW2BJTPkc0dus";
            EmpresaNomeTextBox.Text = "sd";
            EmpresaCnpjTextBox.Text = "33144104000129";
            DeviceNameTextBox.Text = "AplicacaoCosta";
        }
        else
        {
            ClientIdTextBox.Text = configuracao.ClientId;
            EmpresaNomeTextBox.Text = configuracao.EmpresaNome;
            EmpresaCnpjTextBox.Text = configuracao.EmpresaCnpj;
            DeviceNameTextBox.Text = configuracao.DeviceName;
            SecretResultadoTextBox.Text = configuracao.SecretGerado;
        }

        AtualizarUrl();
    }

    private void Campo_Alterado(object? sender, TextChangedEventArgs e) => AtualizarUrl();

    private void AtualizarUrl()
    {
        var url = MontarUrl();
        UrlTextBox.Text = url;
    }

    private string MontarUrl()
    {
        var clientId = ClientIdTextBox.Text?.Trim() ?? string.Empty;
        var empresaNome = EmpresaNomeTextBox.Text?.Trim() ?? string.Empty;
        var empresaCnpj = EmpresaCnpjTextBox.Text?.Trim() ?? string.Empty;
        var deviceName = DeviceNameTextBox.Text?.Trim() ?? string.Empty;

        return $"{EndpointBase}?client_id={Uri.EscapeDataString(clientId)}&empresa_name={Uri.EscapeDataString(empresaNome)}&empresa_cnpj={Uri.EscapeDataString(empresaCnpj)}&device_name={Uri.EscapeDataString(deviceName)}";
    }

    private bool ValidarFormulario()
    {
        if (string.IsNullOrWhiteSpace(ClientIdTextBox.Text))
        {
            MensagemStatus.Text = "Informe o client_id.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(EmpresaNomeTextBox.Text))
        {
            MensagemStatus.Text = "Informe o nome da empresa.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(EmpresaCnpjTextBox.Text))
        {
            MensagemStatus.Text = "Informe o CNPJ da empresa.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(DeviceNameTextBox.Text))
        {
            MensagemStatus.Text = "Informe o nome do dispositivo.";
            return false;
        }

        MensagemStatus.Text = string.Empty;
        return true;
    }

    private static string GerarSecretNoFormatoClientId(int tamanho = 43)
    {
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        var gerador = RandomNumberGenerator.Create();
        var buffer = new byte[tamanho];
        gerador.GetBytes(buffer);

        var builder = new StringBuilder(tamanho);
        for (var indice = 0; indice < tamanho; indice++)
        {
            var valor = buffer[indice] % caracteres.Length;
            builder.Append(caracteres[valor]);
        }

        return builder.ToString();
    }

    private static string GerarTokenValido24h()
    {
        static string Base64UrlEncode(string text) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(text))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

        var agora = DateTimeOffset.UtcNow;
        var expiraEm = agora.AddHours(24);

        var header = Base64UrlEncode("{\"alg\":\"HS256\",\"typ\":\"JWT\"}");
        var payload = Base64UrlEncode(
            $"{{\"sub\":\"{Guid.NewGuid():N}\",\"iss\":\"MeuAppAvalonia\",\"iat\":{agora.ToUnixTimeSeconds()},\"exp\":{expiraEm.ToUnixTimeSeconds()}}}");
        var assinatura = Base64UrlEncode(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

        return $"{header}.{payload}.{assinatura}";
    }

    private async void GerarFormatoButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!ValidarFormulario())
            return;

        var secretGerado = GerarSecretNoFormatoClientId();
        SecretResultadoTextBox.Text = secretGerado;

        MensagemStatus.Text = "Formato do secret gerado. Clique em Salvar configuração para persistir.";
        MensagemStatus.Foreground = Brushes.Orange;
    }

    private async void GerarToken24hButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!ValidarFormulario())
            return;

        var expiraEm = DateTime.UtcNow.AddHours(24);
        var token = GerarTokenValido24h();

        TokenAcessoTextBox.Text = token;
        TokenExpiraEmTextBox.Text = expiraEm.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");

        MensagemStatus.Text = "Token de 24 horas gerado e pronto para salvar.";
        MensagemStatus.Foreground = Brushes.Green;
    }

    private async void CopiarTokenButton_Click(object? sender, RoutedEventArgs e)
    {
        var token = TokenAcessoTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            MensagemStatus.Text = "Gere primeiro o token de autenticação antes de copiar.";
            MensagemStatus.Foreground = Brushes.Orange;
            return;
        }

        try
        {
            WinFormsClipboard.SetText(token);
            MensagemStatus.Text = "Token copiado para a área de transferência.";
            MensagemStatus.Foreground = Brushes.Green;
        }
        catch (Exception ex)
        {
            MensagemStatus.Text = $"Erro ao copiar token: {ex.Message}";
            MensagemStatus.Foreground = Brushes.Red;
        }
    }

    private async void SalvarConfiguracaoButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!ValidarFormulario())
            return;

        var secretGerado = SecretResultadoTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(secretGerado))
        {
            secretGerado = GerarSecretNoFormatoClientId();
            SecretResultadoTextBox.Text = secretGerado;
        }

        var tokenAcesso = TokenAcessoTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(tokenAcesso))
        {
            tokenAcesso = GerarTokenValido24h();
            TokenAcessoTextBox.Text = tokenAcesso;
        }

        var expiraEm = DateTime.TryParse(TokenExpiraEmTextBox.Text, out var expiracao)
            ? expiracao.ToUniversalTime()
            : DateTime.UtcNow.AddHours(24);

        var url = MontarUrl();

        await _configuracaoSecretService.SalvarAsync(
            ClientIdTextBox.Text ?? string.Empty,
            EmpresaNomeTextBox.Text ?? string.Empty,
            EmpresaCnpjTextBox.Text ?? string.Empty,
            DeviceNameTextBox.Text ?? string.Empty,
            secretGerado,
            url,
            tokenAcesso,
            expiraEm);

        MensagemStatus.Text = "Configuração salva com sucesso no banco.";
        MensagemStatus.Foreground = Brushes.Green;
    }

    private void AbrirNoNavegadorButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!ValidarFormulario())
            return;

        var url = MontarUrl();
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

}
