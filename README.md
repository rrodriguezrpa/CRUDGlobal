# Gestion de Empleados

Aplicacion de escritorio **+ API REST** para gestionar empleados de una empresa (RRHH):
alta, consulta, edicion y baja. Pensada como **banco de pruebas para automatizacion con
UiPath**: incluye dificultades habituales del software empresarial que se pueden resolver
de forma robusta con UiPath, tanto por **interfaz de escritorio** como por **API**.

Todo funciona con **un solo ejecutable**. No hay que instalar nada: ni bases de datos, ni
servidores, ni runtimes. Doble-click y listo.

---

## Instalacion

1. Entra en la pestana **[Releases](../../releases)** del repositorio.
2. Descarga **`Empleados.Desktop.exe`**.
3. Doble-click.

Se abre la pantalla de **acceso** y, a la vez, queda disponible una API en
`http://localhost:5230`. La primera vez se crea automaticamente una base de datos local con
15 empleados de ejemplo (en `%LOCALAPPDATA%\EmpleadosApp`).

**Credenciales de demo:**

| Usuario | Contrasena | Rol |
|---------|------------|-----|
| `admin` | `admin123` | Admin |
| `rrhh`  | `rrhh123`  | Usuario |

> No requiere permisos de administrador. Windows SmartScreen puede avisar por ser un
> ejecutable sin firma digital: pulsa *Mas informacion > Ejecutar de todos modos*.

---

## Como se usa

La interfaz de escritorio y la API **comparten la misma base de datos**. Lo que crees por la
API aparece en la tabla (al refrescar) y al reves. Eso permite probar la automatizacion por
los dos caminos contra los mismos datos.

### Por interfaz de escritorio

Tabla de empleados con:
- **Busqueda** por nombre, apellidos, DNI, email o numero de empleado.
- **Filtro** por departamento.
- **Paginacion** (5 / 10 / 20 filas).
- Botones **Nuevo**, **Editar** (o doble-click en una fila) y **Eliminar**.

### Por API REST

Con la app abierta, la API esta en `http://localhost:5230`.
Documentacion interactiva (Swagger): **`http://localhost:5230/swagger`**.

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET    | `/api/empleados?page=1&pageSize=10&search=&departamento=` | Lista paginada |
| GET    | `/api/empleados/{id}` | Obtener un empleado |
| POST   | `/api/empleados` | Crear (el numero de empleado lo asigna el servidor) |
| PUT    | `/api/empleados/{id}` | Actualizar |
| DELETE | `/api/empleados/{id}` | Eliminar |
| GET    | `/health` | Estado del servicio |

Departamentos: `0=RRHH, 1=IT, 2=Ventas, 3=Finanzas, 4=Operaciones`.

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

## Dificultades incluidas a proposito

El objetivo del demo es mostrar como UiPath supera obstaculos que hacen fallar a una
automatizacion ingenua (clicks por coordenadas, selectores fijos, recorrer datos a ciegas).

**En la interfaz de escritorio:**

0. **Login + aviso de sesion.** Antes de operar hay que autenticarse, y al abrir la pantalla
   aparece un **aviso de seguridad** que se debe cerrar primero. Un bot que va directo a teclear
   falla si no gestiona el modal y las credenciales.
   → *UiPath lo resuelve con gestion de credenciales (Assets/Credential, secure string) y manejo de ventanas.*

1. **Selectores dinamicos.** El identificador interno (`AutomationId`) de cada control
   cambia en cada arranque, y el titulo de la ventana lleva un codigo de sesion variable.
   Una grabacion con selectores fijos deja de funcionar al reabrir la app.
   → *UiPath lo resuelve con anchors, fuzzy selectors y atributos estables (texto, rol, indice).*

2. **Ventanas de confirmacion (popups).** Al eliminar, el dialogo muestra los botones en
   **orden y posicion aleatorios**, y a veces pide una **segunda confirmacion**.
   → *UiPath lo resuelve haciendo click por texto/rol y gestionando ventanas modales.*

3. **Datos paginados.** La tabla muestra los empleados por paginas; para obtener todos hay
   que navegar entre ellas.
   → *UiPath lo resuelve con extraccion de datos y navegacion entre paginas, o consumiendo la API.*

**En la API (errores y validaciones tipicos del backend empresarial):**

- **DNI espanol** validado (8 digitos + letra de control) → error `400` con detalle.
- **Duplicados** de DNI o email → `409 Conflict`.
- **Rangos** de salario (12.000–250.000 €) y campos obligatorios → `400`.
- El **numero de empleado lo asigna el servidor**, no el cliente.

---

## Arquitectura

```
Empleados.Desktop.exe  (un solo binario autocontenido)
├── Interfaz de escritorio (WPF)
└── API REST embebida (ASP.NET Core)  ->  base de datos SQLite local
        http://localhost:5230            (%LOCALAPPDATA%\EmpleadosApp\empleados.db)
```

Stack: .NET 9, WPF, ASP.NET Core, Entity Framework Core, SQLite, Swagger.

El codigo fuente esta en `src/`. Para compilar desde fuente y generar releases, ver
[`docs/NOTAS-INTERNAS.md`](docs/NOTAS-INTERNAS.md).
