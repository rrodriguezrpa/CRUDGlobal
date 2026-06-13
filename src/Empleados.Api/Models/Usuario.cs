namespace Empleados.Api.Models;

/// <summary>Usuario de acceso a la aplicacion de escritorio (login).</summary>
public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = "Usuario";
}
