using System.ComponentModel.DataAnnotations;

namespace Empleados.Api.Models;

public enum Departamento
{
    RRHH = 0,
    IT = 1,
    Ventas = 2,
    Finanzas = 3,
    Operaciones = 4
}

public class Empleado
{
    public int Id { get; set; }

    // NumeroEmpleado asignado por el servidor (EMP-0001). Unico.
    public string NumeroEmpleado { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 80 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    // DNI espanol: 8 digitos + letra de control. Validado en el servidor.
    [Required(ErrorMessage = "El DNI es obligatorio.")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato valido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El departamento es obligatorio.")]
    public Departamento Departamento { get; set; }

    [Required(ErrorMessage = "El puesto es obligatorio.")]
    [StringLength(60, MinimumLength = 2)]
    public string Puesto { get; set; } = string.Empty;

    [Range(12000, 250000, ErrorMessage = "El salario debe estar entre 12.000 y 250.000 EUR.")]
    public decimal Salario { get; set; }

    public DateTime FechaAlta { get; set; } = DateTime.Today;

    public bool Activo { get; set; } = true;
}
