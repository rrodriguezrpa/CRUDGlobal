using Empleados.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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

            // Documenta el header X-Api-Key como esquema de seguridad tipo ApiKey
            // para que el spec exportado lo refleje y Connector Builder lo detecte.
            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Name = "X-Api-Key",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Clave de API. Enviar en el header X-Api-Key."
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // CORS: por defecto AllowAnyOrigin (demo). Si ALLOWED_ORIGINS esta definida
        // (lista separada por comas), se restringe a esos origenes.
        var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        {
            if (string.IsNullOrWhiteSpace(allowedOrigins))
                p.AllowAnyOrigin();
            else
                p.WithOrigins(allowedOrigins.Split(',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            p.AllowAnyMethod().AllowAnyHeader();
        }));

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DbSeeder.Seed(db);
        }

        // PUBLIC_BASE_URL (URL del tunel) se anade como server del documento OpenAPI.
        var publicBaseUrl = Environment.GetEnvironmentVariable("PUBLIC_BASE_URL");
        app.UseSwagger(c =>
        {
            if (!string.IsNullOrWhiteSpace(publicBaseUrl))
            {
                c.PreSerializeFilters.Add((doc, _) =>
                    doc.Servers = new List<OpenApiServer> { new() { Url = publicBaseUrl } });
            }
        });
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Empleados v1");
            c.RoutePrefix = "swagger";
        });

        app.UseCors();

        // API key: protege todo salvo /swagger, /health y la raiz "/".
        // Clave desde DEMO_API_KEY; por defecto "demo-uipath-2026". Header: X-Api-Key.
        var apiKey = Environment.GetEnvironmentVariable("DEMO_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) apiKey = "demo-uipath-2026";

        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var abierto = path == "/"
                || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase);

            if (!abierto)
            {
                var provista = context.Request.Headers["X-Api-Key"].ToString();
                if (!string.Equals(provista, apiKey, StringComparison.Ordinal))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(
                        new { error = "API key inválida o ausente" });
                    return;
                }
            }

            await next();
        });

        app.MapControllers();
        app.MapGet("/", () => Results.Redirect("/swagger"));
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        return app;
    }
}
