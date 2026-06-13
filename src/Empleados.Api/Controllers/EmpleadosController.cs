using Empleados.Api.Data;
using Empleados.Api.Dtos;
using Empleados.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Empleados.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmpleadosController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmpleadosController(AppDbContext db) => _db = db;

    /// <summary>Lista paginada de empleados con busqueda y filtro por departamento.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Empleado>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Empleado>>> Get(
        [FromQuery] string? search,
        [FromQuery] Departamento? departamento,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 10;

        var query = _db.Empleados.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(e =>
                e.Nombre.Contains(s) ||
                e.Apellidos.Contains(s) ||
                e.NumeroEmpleado.Contains(s) ||
                e.Dni.Contains(s) ||
                e.Email.Contains(s));
        }

        if (departamento.HasValue)
            query = query.Where(e => e.Departamento == departamento.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<Empleado>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        });
    }

    /// <summary>Obtiene un empleado por Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Empleado), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Empleado>> GetById(int id)
    {
        var empleado = await _db.Empleados.FindAsync(id);
        return empleado is null ? NotFound() : Ok(empleado);
    }

    /// <summary>Crea un empleado. El servidor asigna NumeroEmpleado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Empleado), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Empleado>> Create([FromBody] EmpleadoInput input)
    {
        var error = await Validar(input, null);
        if (error is not null) return error;

        var empleado = new Empleado
        {
            NumeroEmpleado = await SiguienteNumeroEmpleado(),
            Nombre = input.Nombre.Trim(),
            Apellidos = input.Apellidos.Trim(),
            Dni = DniValidator.Normalizar(input.Dni),
            Email = input.Email.Trim(),
            Departamento = input.Departamento,
            Puesto = input.Puesto.Trim(),
            Salario = input.Salario,
            FechaAlta = DateTime.Today,
            Activo = input.Activo
        };

        _db.Empleados.Add(empleado);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = empleado.Id }, empleado);
    }

    /// <summary>Actualiza un empleado existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Empleado), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Empleado>> Update(int id, [FromBody] EmpleadoInput input)
    {
        var empleado = await _db.Empleados.FindAsync(id);
        if (empleado is null) return NotFound();

        var error = await Validar(input, id);
        if (error is not null) return error;

        empleado.Nombre = input.Nombre.Trim();
        empleado.Apellidos = input.Apellidos.Trim();
        empleado.Dni = DniValidator.Normalizar(input.Dni);
        empleado.Email = input.Email.Trim();
        empleado.Departamento = input.Departamento;
        empleado.Puesto = input.Puesto.Trim();
        empleado.Salario = input.Salario;
        empleado.Activo = input.Activo;

        await _db.SaveChangesAsync();
        return Ok(empleado);
    }

    /// <summary>Elimina un empleado por Id.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var empleado = await _db.Empleados.FindAsync(id);
        if (empleado is null) return NotFound();

        _db.Empleados.Remove(empleado);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---- helpers ----

    private async Task<ActionResult?> Validar(EmpleadoInput input, int? idActual)
    {
        if (!DniValidator.EsValido(input.Dni))
        {
            ModelState.AddModelError(nameof(input.Dni),
                "El DNI no es valido: debe ser 8 digitos y la letra de control correcta.");
            return ValidationProblem(ModelState);
        }

        var dniNorm = DniValidator.Normalizar(input.Dni);
        var dniDuplicado = await _db.Empleados
            .AnyAsync(e => e.Dni == dniNorm && e.Id != idActual);
        if (dniDuplicado)
            return Conflict(new { mensaje = $"Ya existe un empleado con el DNI {dniNorm}." });

        var emailNorm = input.Email.Trim();
        var emailDuplicado = await _db.Empleados
            .AnyAsync(e => e.Email == emailNorm && e.Id != idActual);
        if (emailDuplicado)
            return Conflict(new { mensaje = $"Ya existe un empleado con el email {emailNorm}." });

        return null;
    }

    private async Task<string> SiguienteNumeroEmpleado()
    {
        var ultimo = await _db.Empleados
            .OrderByDescending(e => e.Id)
            .Select(e => e.NumeroEmpleado)
            .FirstOrDefaultAsync();

        var n = 1;
        if (!string.IsNullOrEmpty(ultimo) && ultimo.StartsWith("EMP-")
            && int.TryParse(ultimo[4..], out var prev))
        {
            n = prev + 1;
        }
        return $"EMP-{n:0000}";
    }
}
