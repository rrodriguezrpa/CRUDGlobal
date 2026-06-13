using Empleados.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Empleados.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();
        SeedUsuarios(db);
        if (db.Empleados.Any()) return;

        // DNIs con letra de control correcta (modulo 23).
        var empleados = new[]
        {
            new Empleado { NumeroEmpleado = "EMP-0001", Nombre = "Lucia",   Apellidos = "Garcia Moreno",   Dni = "12345678Z", Email = "lucia.garcia@empresa.com",  Departamento = Departamento.RRHH,        Puesto = "Tecnico de RRHH",        Salario = 32000, FechaAlta = new DateTime(2019, 3, 12), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0002", Nombre = "Carlos",  Apellidos = "Fernandez Ruiz",   Dni = "23456789R", Email = "carlos.fernandez@empresa.com", Departamento = Departamento.IT,          Puesto = "Desarrollador Senior",   Salario = 48000, FechaAlta = new DateTime(2018, 7, 1),  Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0003", Nombre = "Marta",   Apellidos = "Lopez Diaz",       Dni = "34567890W", Email = "marta.lopez@empresa.com",     Departamento = Departamento.Ventas,      Puesto = "Comercial",              Salario = 28000, FechaAlta = new DateTime(2021, 1, 20), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0004", Nombre = "Javier",  Apellidos = "Martin Sanz",      Dni = "45678901A", Email = "javier.martin@empresa.com",   Departamento = Departamento.Finanzas,    Puesto = "Contable",               Salario = 35000, FechaAlta = new DateTime(2017, 11, 5), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0005", Nombre = "Ana",     Apellidos = "Sanchez Gomez",    Dni = "56789012G", Email = "ana.sanchez@empresa.com",     Departamento = Departamento.Operaciones, Puesto = "Jefe de Operaciones",    Salario = 52000, FechaAlta = new DateTime(2016, 5, 18), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0006", Nombre = "David",   Apellidos = "Romero Castro",    Dni = "67890123M", Email = "david.romero@empresa.com",    Departamento = Departamento.IT,          Puesto = "Administrador de Sistemas", Salario = 41000, FechaAlta = new DateTime(2020, 9, 30), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0007", Nombre = "Elena",   Apellidos = "Navarro Vidal",    Dni = "78901234Y", Email = "elena.navarro@empresa.com",   Departamento = Departamento.Ventas,      Puesto = "Responsable de Ventas",  Salario = 46000, FechaAlta = new DateTime(2015, 2, 9),  Activo = false },
            new Empleado { NumeroEmpleado = "EMP-0008", Nombre = "Pablo",   Apellidos = "Iglesias Cano",    Dni = "89012345F", Email = "pablo.iglesias@empresa.com",  Departamento = Departamento.Finanzas,    Puesto = "Analista Financiero",    Salario = 38000, FechaAlta = new DateTime(2022, 6, 14), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0009", Nombre = "Sara",    Apellidos = "Ortega Pena",      Dni = "90123456P", Email = "sara.ortega@empresa.com",     Departamento = Departamento.RRHH,        Puesto = "Responsable de RRHH",    Salario = 50000, FechaAlta = new DateTime(2014, 10, 2), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0010", Nombre = "Hugo",    Apellidos = "Delgado Marin",    Dni = "01234567L", Email = "hugo.delgado@empresa.com",    Departamento = Departamento.Operaciones, Puesto = "Operario",               Salario = 22000, FechaAlta = new DateTime(2023, 4, 25), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0011", Nombre = "Claudia", Apellidos = "Ramos Herrera",    Dni = "11223344A", Email = "claudia.ramos@empresa.com",   Departamento = Departamento.IT,          Puesto = "QA Engineer",            Salario = 36000, FechaAlta = new DateTime(2021, 8, 11), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0012", Nombre = "Sergio",  Apellidos = "Vega Torres",      Dni = "22334455L", Email = "sergio.vega@empresa.com",     Departamento = Departamento.Ventas,      Puesto = "Comercial Junior",       Salario = 24000, FechaAlta = new DateTime(2023, 1, 16), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0013", Nombre = "Natalia", Apellidos = "Cabrera Soler",    Dni = "33445566C", Email = "natalia.cabrera@empresa.com", Departamento = Departamento.Finanzas,    Puesto = "Tesoreria",              Salario = 33000, FechaAlta = new DateTime(2019, 12, 3), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0014", Nombre = "Adrian",  Apellidos = "Flores Mendez",    Dni = "44556677E", Email = "adrian.flores@empresa.com",   Departamento = Departamento.Operaciones, Puesto = "Logistica",              Salario = 27000, FechaAlta = new DateTime(2020, 3, 22), Activo = true },
            new Empleado { NumeroEmpleado = "EMP-0015", Nombre = "Beatriz", Apellidos = "Crespo Aguilar",   Dni = "55667788K", Email = "beatriz.crespo@empresa.com",  Departamento = Departamento.RRHH,        Puesto = "Tecnico de Seleccion",   Salario = 30000, FechaAlta = new DateTime(2022, 11, 7), Activo = true },
        };

        db.Empleados.AddRange(empleados);
        db.SaveChanges();
    }

    private static void SeedUsuarios(AppDbContext db)
    {
        // Crea la tabla si una version anterior de la BD (sin login) no la tenia.
        // Evita "no such table: Usuarios" al actualizar sobre una BD existente.
        db.Database.ExecuteSqlRaw(
            @"CREATE TABLE IF NOT EXISTS ""Usuarios"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Usuarios"" PRIMARY KEY AUTOINCREMENT,
                ""Username"" TEXT NOT NULL,
                ""PasswordHash"" TEXT NOT NULL,
                ""NombreCompleto"" TEXT NOT NULL,
                ""Rol"" TEXT NOT NULL);
              CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Usuarios_Username"" ON ""Usuarios"" (""Username"");");

        if (db.Usuarios.Any()) return;

        // Credenciales de demo (ver pantalla de login).
        db.Usuarios.AddRange(
            new Usuario { Username = "admin", PasswordHash = PasswordHasher.Hash("admin123"),
                          NombreCompleto = "Administrador", Rol = "Admin" },
            new Usuario { Username = "rrhh",  PasswordHash = PasswordHasher.Hash("rrhh123"),
                          NombreCompleto = "Tecnico de RRHH", Rol = "Usuario" });
        db.SaveChanges();
    }
}
