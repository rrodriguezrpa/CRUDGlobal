# Publica la app de escritorio como UN SOLO .exe autocontenido (sin dependencias para el usuario).
# Uso:  .\publish.ps1            -> genera .\dist\Empleados.Desktop.exe
#       .\publish.ps1 -Zip       -> ademas crea .\dist\EmpleadosApp-win-x64.zip
param(
    [switch]$Zip
)

$ErrorActionPreference = "Stop"
$proj = "src\Empleados.Desktop\Empleados.Desktop.csproj"
$out  = "dist"

Write-Host "Publicando single-file self-contained (win-x64)..." -ForegroundColor Cyan
dotnet publish $proj -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none `
    -o $out

# Solo el .exe es necesario para ejecutar; el resto son metadatos opcionales.
$exe = Join-Path $out "Empleados.Desktop.exe"
if (-not (Test-Path $exe)) { throw "No se genero el exe." }
$mb = [math]::Round((Get-Item $exe).Length / 1MB, 1)
Write-Host "OK -> $exe ($mb MB)" -ForegroundColor Green

if ($Zip) {
    $zip = Join-Path $out "EmpleadosApp-win-x64.zip"
    if (Test-Path $zip) { Remove-Item $zip -Force }
    Compress-Archive -Path $exe -DestinationPath $zip
    Write-Host "ZIP -> $zip" -ForegroundColor Green
}
