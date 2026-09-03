using Avalonia;
using System;

namespace MeuAppAvalonia;

class Programa
{
    // O código de inicialização deve ser executado somente após o Avalonia estar pronto.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Configuração do Avalonia, também usada pelo designer visual.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<Aplicacao>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
