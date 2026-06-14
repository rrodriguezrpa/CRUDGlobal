# Demo API con API key + tunel

API protegida por clave en el header `X-Api-Key`. Abierto sin clave: `/swagger`, `/health`, `/`.

## Arranque (PowerShell, carpeta `src/Empleados.Api`)

```powershell
$env:DEMO_API_KEY   = "demo-uipath-2026"
$env:PUBLIC_BASE_URL = "https://xxxx.ts.net"   # URL del tunel (opcional)
$env:ALLOWED_ORIGINS = ""                       # vacio = cualquier origen
dotnet run
```

Escucha en `http://localhost:5230`. Swagger UI: `/swagger`. Spec: `/swagger/v1/swagger.json`.

## Probar

```powershell
curl http://localhost:5230/health                                  # 200 sin key
curl http://localhost:5230/api/empleados                           # 401 sin key
curl -H "X-Api-Key: demo-uipath-2026" http://localhost:5230/api/empleados   # 200
```

`PUBLIC_BASE_URL` se inyecta como `server` del OpenAPI para que Connector Builder use la URL del tunel.
