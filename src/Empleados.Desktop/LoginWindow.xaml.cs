using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Empleados.Api.Dtos;

namespace Empleados.Desktop;

public partial class LoginWindow : Window
{
    private readonly ApiClient _api;

    /// <summary>Usuario autenticado (null si no se completo el login).</summary>
    public LoginResponse? UsuarioAutenticado { get; private set; }

    public LoginWindow(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        AplicarSelectoresDinamicos();
        Loaded += LoginWindow_Loaded;
    }

    private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // BARRERA: aviso de sesion / terminos que hay que aceptar antes de operar.
        // Un bot que va directo a teclear usuario falla si no cierra este modal primero.
        ConfirmDialog.Aviso(this,
            "AVISO DE SEGURIDAD\n\n" +
            "Este sistema es de uso exclusivo para personal autorizado. El acceso queda " +
            "registrado. Al continuar acepta las condiciones de uso.");

        TxtUsuario.Focus();
    }

    // BARRERA: AutomationId aleatorio por sesion tambien en el login.
    private void AplicarSelectoresDinamicos()
    {
        SelectorObfuscator.AleatorizarTodos(
            (TxtUsuario, "txtUsuario"), (TxtPassword, "txtPassword"), (BtnEntrar, "btnEntrar"));
    }

    private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) BtnEntrar_Click(sender, e);
    }

    private async void BtnEntrar_Click(object sender, RoutedEventArgs e)
    {
        var usuario = TxtUsuario.Text.Trim();
        var clave = TxtPassword.Password;

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
        {
            MostrarError("Introduzca usuario y contrasena.");
            return;
        }

        BtnEntrar.IsEnabled = false;
        BtnEntrar.Content = "Validando...";

        var res = await _api.LoginAsync(usuario, clave);

        BtnEntrar.IsEnabled = true;
        BtnEntrar.Content = "Entrar";

        if (res.Ok && res.Value is not null)
        {
            UsuarioAutenticado = res.Value;
            DialogResult = true;
        }
        else
        {
            TxtPassword.Clear();
            MostrarError(res.Error ?? "No se pudo iniciar sesion.");
        }
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.Visibility = Visibility.Visible;
    }
}
