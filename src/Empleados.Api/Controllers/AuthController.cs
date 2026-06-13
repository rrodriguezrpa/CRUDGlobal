using Empleados.Api.Data;
using Empleados.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Empleados.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db) => _db = db;

    /// <summary>Valida credenciales de acceso a la app de escritorio.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Username == req.Username);

        if (user is null || !PasswordHasher.Verificar(req.Password, user.PasswordHash))
            return Unauthorized(new { mensaje = "Usuario o contrasena incorrectos." });

        return Ok(new LoginResponse
        {
            Username = user.Username,
            NombreCompleto = user.NombreCompleto,
            Rol = user.Rol
        });
    }
}
