using Empleados.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Empleados.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<Empleado>();
        e.HasIndex(x => x.Dni).IsUnique();
        e.HasIndex(x => x.NumeroEmpleado).IsUnique();
        e.Property(x => x.Salario).HasColumnType("decimal(10,2)");
        e.Property(x => x.Departamento).HasConversion<int>();

        modelBuilder.Entity<Usuario>().HasIndex(x => x.Username).IsUnique();
    }
}
