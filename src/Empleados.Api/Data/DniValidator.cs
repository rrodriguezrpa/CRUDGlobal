using System.Text.RegularExpressions;

namespace Empleados.Api.Data;

public static partial class DniValidator
{
    private const string LetraControl = "TRWAGMYFPDXBNJZSQVHLCKE";

    [GeneratedRegex(@"^\d{8}[A-Za-z]$")]
    private static partial Regex DniRegex();

    // Valida DNI espanol: 8 digitos + letra correcta segun el modulo 23.
    public static bool EsValido(string? dni)
    {
        if (string.IsNullOrWhiteSpace(dni)) return false;
        dni = dni.Trim().ToUpperInvariant();
        if (!DniRegex().IsMatch(dni)) return false;

        var numero = int.Parse(dni[..8]);
        var letraEsperada = LetraControl[numero % 23];
        return dni[8] == letraEsperada;
    }

    public static string Normalizar(string dni) => dni.Trim().ToUpperInvariant();
}
