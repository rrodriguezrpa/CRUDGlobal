using Empleados.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Empleados.Api;

public static class ApiHost
{
    public const string DefaultUrl = "http://localhost:5230";

    /// <summary>Ruta del fichero SQLite en %LOCALAPPDATA%\EmpleadosApp\empleados.db</summary>
    public static string DefaultDbPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EmpleadosApp");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "empleados.db");
    }

    /// <summary>Construye la WebApplication (API + Swagger + SQLite). No la arranca.</summary>
    public static WebApplication Build(string? url = null, string? dbPath = null)
    {
        dbPath ??= DefaultDbPath();
        url ??= DefaultUrl;

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            // Evita depender del cwd cuando se ejecuta embebido en la app de escritorio.
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.WebHost.UseUrls(url);
        builder.Logging.ClearProviders();

        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite($"Data Source={dbPath}"));

        // AddApplicationPart: necesario cuando la API corre embebida en la app de escritorio,
        // porque el assembly de entrada es el WPF exe y los controllers viven en este assembly.
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(ApiHost).Assembly);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "API Gestion de Empleados", Version = "v1" });
        });

        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DbSeeder.Seed(db);
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Empleados v1");
            c.RoutePrefix = "swagger";
        });

        app.UseCors();
        app.MapControllers();
        app.MapGet("/", () => Results.Redirect("/swagger"));
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        return app;
    }
}
