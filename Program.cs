using System;
using Avalonia;

namespace MeuAppAvalonia;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<Aplicacao>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}
