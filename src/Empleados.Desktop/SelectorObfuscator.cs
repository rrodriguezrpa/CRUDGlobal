using System.Windows;
using System.Windows.Automation;

namespace Empleados.Desktop;

/// <summary>
/// BARRERA ANTI-AUTOMATIZACION (demo UiPath): asigna AutomationId aleatorios por sesion.
/// Un selector grabado con id fijo deja de funcionar al reabrir la app.
/// UiPath lo resuelve con anchors, fuzzy selectors y atributos estables (idx, role, texto).
/// </summary>
public static class SelectorObfuscator
{
    /// <summary>Token unico de la sesion actual (cambia en cada arranque).</summary>
    public static readonly string SessionToken =
        Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

    private static readonly Random Rng = new();

    /// <summary>Asigna un AutomationId aleatorio (prefijo + ruido) al control.</summary>
    public static void Aleatorizar(DependencyObject control, string prefijo)
    {
        var ruido = Rng.Next(100000, 999999);
        AutomationProperties.SetAutomationId(control, $"{prefijo}_{SessionToken}_{ruido}");
    }

    /// <summary>Aplica ids aleatorios a varios controles de golpe.</summary>
    public static void AleatorizarTodos(params (DependencyObject Control, string Prefijo)[] controles)
    {
        foreach (var (control, prefijo) in controles)
            Aleatorizar(control, prefijo);
    }
}
