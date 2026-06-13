using System.ComponentModel.DataAnnotations;
using Empleados.Api.Models;

namespace Empleados.Api.Dtos;

// DTO de entrada: el cliente NO envia Id ni NumeroEmpleado (los pone el servidor).
public class EmpleadoInput
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(80, MinimumLength = 2)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El departamento es obligatorio.")]
    public Departamento Departamento { get; set; }

    [Required(ErrorMessage = "El puesto es obligatorio.")]
    [StringLength(60, MinimumLength = 2)]
    public string Puesto { get; set; } = string.Empty;

    [Range(12000, 250000)]
    public decimal Salario { get; set; }

    public bool Activo { get; set; } = true;
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalItems / (double)PageSize) : 0;
}
