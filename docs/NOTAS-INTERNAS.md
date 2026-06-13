# Notas internas (mantenimiento)

Documento para quien desarrolla/publica el proyecto. **No** es para la audiencia del demo.

## Requisitos
- **.NET 9 SDK** (solo para compilar; el usuario final no lo necesita).

## Estructura
```
src/Empleados.Api/       Web API (EF Core + SQLite, controllers, Swagger).
                         Reutilizable como ApiHost.Build(...). Ejecutable en solitario.
src/Empleados.Desktop/   WPF. Arranca la API embebida y la consume via localhost (ApiClient).
publish.ps1              Genera el .exe autocontenido en .\dist
.github/workflows/       Build + Release automatica al pushear un tag vX.Y.Z
```

La API corre embebida dentro del WPF: por eso `ApiHost` usa `AddApplicationPart(...)` (el
assembly de entrada es el exe de escritorio, no el de la API, y si no los controllers no se
descubren → 404).

## Ejecutar en desarrollo
```powershell
dotnet run --project src/Empleados.Desktop   # UI + API embebida
dotnet run --project src/Empleados.Api       # solo API (pruebas de API)
```

## Generar el .exe
```powershell
.\publish.ps1            # solo dist\Empleados.Desktop.exe
.\publish.ps1 -Zip       # ademas dist\EmpleadosApp-win-x64.zip
```
Single-file, self-contained, win-x64 (~74 MB). `dist/` esta en `.gitignore`: los binarios no
van al repo, van a **Releases**.

## Publicar una Release

### Opcion A — automatica (recomendada)
Pushea un tag y GitHub Actions compila y adjunta el exe:
```powershell
git push origin main
git tag v1.0.0
git push origin v1.0.0
```

### Opcion B — manual
Generar con `.\publish.ps1 -Zip` y subir `dist\Empleados.Desktop.exe` en
`releases/new` (crear tag `vX.Y.Z` y adjuntar el binario).

## Notas de Git / GitHub
- Push por HTTPS necesita **Personal Access Token** (classic) con scopes **`repo`** y
  **`workflow`** (este ultimo porque el repo contiene `.github/workflows/`).
- Al hacer push: usuario = cuenta de GitHub, contrasena = el token (no la contrasena real).
- Si cambia el token, borrar la credencial cacheada:
  `"protocol=https`nhost=github.com`n" | git credential-manager erase`

## Pendientes / mejoras posibles
- Firmar el exe (evita aviso de SmartScreen).
- Icono de aplicacion.
- 4a barrera: campos condicionales (campo extra obligatorio segun departamento).
- Login en la app (barrera adicional realista).
