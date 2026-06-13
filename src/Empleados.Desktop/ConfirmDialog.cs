using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Empleados.Desktop;

/// <summary>
/// BARRERA ANTI-AUTOMATIZACION (demo UiPath): modal de confirmacion donde el orden y la
/// posicion de los botones cambian de forma aleatoria, y el AutomationId es distinto cada vez.
/// Un click grabado por coordenadas o por orden fijo falla; UiPath lo resuelve por texto/role.
/// </summary>
public static class ConfirmDialog
{
    private static readonly Random Rng = new();

    public static bool Confirmar(Window owner, string mensaje)
        => Mostrar(owner, mensaje, "Confirmacion requerida", conCancelar: true);

    public static void Aviso(Window owner, string mensaje)
        => Mostrar(owner, mensaje, "Aviso", conCancelar: false);

    private static bool Mostrar(Window owner, string mensaje, string titulo, bool conCancelar)
    {
        bool resultado = false;

        var win = new Window
        {
            Title = $"{titulo} ::{Guid.NewGuid().ToString("N")[..4]}",
            Width = 420,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.SingleBorderWindow,
            Background = Brushes.White
        };

        var root = new StackPanel { Margin = new Thickness(20) };

        root.Children.Add(new TextBlock
        {
            Text = mensaje,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 18)
        });

        // Contenedor de botones. La alineacion horizontal se decide al azar.
        var alineaciones = new[] { HorizontalAlignment.Right, HorizontalAlignment.Center, HorizontalAlignment.Left };
        var fila = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = alineaciones[Rng.Next(alineaciones.Length)]
        };

        Button Crear(string texto, bool valor, Brush color)
        {
            var b = new Button
            {
                Content = texto,
                MinWidth = 90,
                Margin = new Thickness(6, 0, 6, 0),
                Padding = new Thickness(14, 7, 14, 7),
                Background = color,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            SelectorObfuscator.Aleatorizar(b, "btnDlg");
            b.Click += (_, _) => { resultado = valor; win.DialogResult = true; };
            return b;
        }

        var aceptar = Crear(conCancelar ? "Si, continuar" : "Aceptar", true, new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x5B)));

        if (conCancelar)
        {
            var cancelar = Crear("No, cancelar", false, new SolidColorBrush(Color.FromRgb(0x6C, 0x7A, 0x89)));
            // Orden de los botones aleatorio.
            if (Rng.Next(2) == 0)
            {
                fila.Children.Add(aceptar);
                fila.Children.Add(cancelar);
            }
            else
            {
                fila.Children.Add(cancelar);
                fila.Children.Add(aceptar);
            }
        }
        else
        {
            fila.Children.Add(aceptar);
        }

        root.Children.Add(fila);
        win.Content = root;
        win.ShowDialog();
        return resultado;
    }
}
