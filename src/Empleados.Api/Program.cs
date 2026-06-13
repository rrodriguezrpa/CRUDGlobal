// Punto de entrada para ejecutar SOLO la API (modo standalone / pruebas por API).
// La app de escritorio reutiliza Empleados.Api.ApiHost.Build(...) para hospedarla embebida.
using Empleados.Api;

var app = ApiHost.Build();
app.Run();
