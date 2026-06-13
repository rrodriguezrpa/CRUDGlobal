using System.Windows;
using ApiHostLib = Empleados.Api.ApiHost;

namespace Empleados.Desktop;

/// <summary>
/// Arranca la API embebida (ASP.NET Core + SQLite) en segundo plano y despues abre la ventana.
/// Un unico ejecutable expone UI de escritorio y API REST a la vez.
/// </summary>
public partial class App : Application
{
    private Microsoft.AspNetCore.Builder.WebApplication? _api;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            _api = ApiHostLib.Build();
            await _api.StartAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo iniciar la API embebida.\n\n{ex.Message}",
                "Error de arranque", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        var api = new ApiClient(ApiHostLib.DefaultUrl);

        // Gate de login (solo escritorio). La API CRUD queda abierta a proposito para pruebas.
        var login = new LoginWindow(api);
        if (login.ShowDialog() != true || login.UsuarioAutenticado is null)
        {
            Shutdown(0);
            return;
        }

        var ventana = new MainWindow(api, login.UsuarioAutenticado);
        ventana.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_api is not null)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            try { await _api.StopAsync(cts.Token); } catch { /* cierre best-effort */ }
            await _api.DisposeAsync();
        }
        base.OnExit(e);
    }
}
