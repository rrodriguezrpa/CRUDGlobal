# Gestion de Empleados — Demo UiPath

Aplicacion de escritorio **+ API REST** para gestionar empleados (CRUD empresarial, RRHH).
Pensada para **demostrar las virtudes de UiPath**: incluye barreras tipicas que rompen
las automatizaciones ingenuas y que UiPath resuelve con sus selectores y actividades robustas.

Todo corre en **un solo ejecutable autocontenido**: la app de escritorio hospeda la API
embebida y una base de datos SQLite local. El usuario final **no instala nada** (ni .NET, ni
SQLite, ni servidor). Doble-click y funciona.

---

## Instalacion (usuario final)

1. Ve a la pestana **Releases** del repositorio.
2. Descarga `Empleados.Desktop.exe`.
3. Doble-click. Se abre la ventana y, en paralelo, la API queda disponible en
   `http://localhost:5230`.

No requiere permisos de administrador. La base de datos se crea automaticamente en
`%LOCALAPPDATA%\EmpleadosApp\empleados.db` con 15 empleados de ejemplo.

> Windows SmartScreen puede avisar por ser un exe sin firma. *Mas info > Ejecutar de todos modos.*

---

## Dos formas de probar: escritorio y API

La UI de escritorio y la API REST comparten la **misma base de datos**. Lo que creas por la
API aparece en la tabla al refrescar, y viceversa.

### Por escritorio
Ventana con tabla paginada, busqueda, filtro por departamento y botones Nuevo / Editar / Eliminar.

### Por API (REST)
Con la app abierta, la API esta en `http://localhost:5230`. Swagger UI: `http://localhost:5230/swagger`.

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET    | `/api/empleados?page=1&pageSize=10&search=&departamento=` | Lista paginada |
| GET    | `/api/empleados/{id}` | Obtener uno |
| POST   | `/api/empleados` | Crear (el servidor asigna `NumeroEmpleado`) |
| PUT    | `/api/empleados/{id}` | Actualizar |
| DELETE | `/api/empleados/{id}` | Eliminar |
| GET    | `/health` | Estado del servicio |

`departamento`: `0=RRHH, 1=IT, 2=Ventas, 3=Finanzas, 4=Operaciones`.

Ejemplo (PowerShell):

```powershell
# Listar
Invoke-RestMethod "http://localhost:5230/api/empleados?pageSize=5"

# Crear
$body = @{
  nombre="Marta"; apellidos="Ruiz Soler"; dni="00000010X"
  email="marta.ruiz@empresa.com"; departamento=1; puesto="Analista"
  salario=31000; activo=$true
} | ConvertTo-Json
Invoke-RestMethod "http://localhost:5230/api/empleados" -Method Post -Body $body -ContentType "application/json"
```

---

## Barreras anti-automatizacion (para lucir UiPath)

Estas dificultades estan **puestas a proposito**. Una grabacion ingenua (clicks por
coordenadas, selectores por id fijo) falla; UiPath las resuelve.

1. **Selectores dinamicos.**
   El `AutomationId` de cada control cambia en cada arranque (token de sesion + ruido), y el
   titulo de la ventana incluye un codigo de sesion variable.
   *UiPath:* anchors, fuzzy selectors, atributos estables (texto, role, idx).
   Codigo: `SelectorObfuscator.cs`.

2. **Popups y dialogos modales.**
   Al eliminar aparece una confirmacion con **orden y posicion de botones aleatorios**, y a
   veces una **segunda confirmacion**.
   *UiPath:* click por texto/role, triggers, manejo de ventanas modales.
   Codigo: `ConfirmDialog.cs`.

3. **Tabla paginada.**
   Los datos llegan paginados desde la API; para extraer todo hay que recorrer paginas.
   *UiPath:* Data Scraping con navegacion entre paginas, o consumir la API directamente.

### Barreras del lado API (errores comunes de desarrollo)
- **Validacion de DNI espanol** (8 digitos + letra de control modulo 23) -> `400` con detalle.
- **Duplicados** de DNI o email -> `409 Conflict`.
- **Rangos** de salario (12.000–250.000 €) y campos obligatorios -> `400`.
- **`NumeroEmpleado` lo asigna el servidor**, no el cliente.

---

## Arquitectura

```
Empleados.Desktop.exe  (un solo binario, self-contained)
├── WPF UI (escritorio)
└── ASP.NET Core Web API embebida  ->  SQLite (%LOCALAPPDATA%\EmpleadosApp\empleados.db)
        http://localhost:5230
```

- `src/Empleados.Api` — Web API (EF Core + SQLite, controllers, Swagger). Reutilizable
  como `ApiHost.Build(...)`. Tambien ejecutable en solitario para pruebas de solo-API.
- `src/Empleados.Desktop` — WPF. Arranca la API embebida y consume `localhost` via `ApiClient`.

---

## Compilar desde codigo fuente

Requisitos: **.NET 9 SDK** (solo para compilar; el usuario final no lo necesita).

```powershell
# Ejecutar en desarrollo
dotnet run --project src/Empleados.Desktop

# Solo la API (pruebas de API)
dotnet run --project src/Empleados.Api

# Generar el .exe autocontenido en .\dist
.\publish.ps1            # solo exe
.\publish.ps1 -Zip       # exe + zip
```

### Publicar una Release
Empuja un tag y GitHub Actions construye y adjunta el `.exe`:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

---

## Notas
- Puerto fijo `5230`. Si esta ocupado, la app avisa al arrancar.
- Stack: .NET 9, WPF, ASP.NET Core, EF Core 9, SQLite, Swagger.
