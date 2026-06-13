using System.Security.Cryptography;
using System.Text;

namespace Empleados.Api.Data;

/// <summary>Hash SHA-256 (hex). Suficiente para el demo; no usar tal cual en produccion.</summary>
public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool Verificar(string password, string hash)
        => string.Equals(Hash(password), hash, StringComparison.OrdinalIgnoreCase);
}
