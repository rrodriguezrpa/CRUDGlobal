using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Empleados.Api.Dtos;
using Empleados.Api.Models;

namespace Empleados.Desktop;

/// <summary>Resultado de una operacion contra la API, con mensaje de error legible.</summary>
public record ApiResult(bool Ok, string? Error = null, HttpStatusCode Status = HttpStatusCode.OK);
public record ApiResult<T>(bool Ok, T? Value, string? Error = null, HttpStatusCode Status = HttpStatusCode.OK);

public class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(15) };
    }

    public async Task<ApiResult<PagedResult<Empleado>>> ListarAsync(
        string? search, Departamento? departamento, int page, int pageSize)
    {
        var url = $"api/empleados?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (departamento.HasValue) url += $"&departamento={(int)departamento.Value}";

        try
        {
            var data = await _http.GetFromJsonAsync<PagedResult<Empleado>>(url, JsonOpts);
            return new ApiResult<PagedResult<Empleado>>(true, data);
        }
        catch (Exception ex)
        {
            return new ApiResult<PagedResult<Empleado>>(false, null, ex.Message);
        }
    }

    public async Task<ApiResult> CrearAsync(EmpleadoInput input)
        => await EnviarAsync(HttpMethod.Post, "api/empleados", input);

    public async Task<ApiResult> ActualizarAsync(int id, EmpleadoInput input)
        => await EnviarAsync(HttpMethod.Put, $"api/empleados/{id}", input);

    public async Task<ApiResult> EliminarAsync(int id)
    {
        try
        {
            var resp = await _http.DeleteAsync($"api/empleados/{id}");
            return resp.IsSuccessStatusCode
                ? new ApiResult(true, Status: resp.StatusCode)
                : new ApiResult(false, await LeerError(resp), resp.StatusCode);
        }
        catch (Exception ex) { return new ApiResult(false, ex.Message); }
    }

    private async Task<ApiResult> EnviarAsync(HttpMethod metodo, string url, EmpleadoInput input)
    {
        try
        {
            using var req = new HttpRequestMessage(metodo, url)
            {
                Content = JsonContent.Create(input, options: JsonOpts)
            };
            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode
                ? new ApiResult(true, Status: resp.StatusCode)
                : new ApiResult(false, await LeerError(resp), resp.StatusCode);
        }
        catch (Exception ex) { return new ApiResult(false, ex.Message); }
    }

    // Extrae el mensaje de error del cuerpo: ProblemDetails (validacion) o { mensaje } (conflicto).
    private static async Task<string> LeerError(HttpResponseMessage resp)
    {
        var body = await resp.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
            return $"Error {(int)resp.StatusCode} {resp.ReasonPhrase}";

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var msgs = new List<string>();
                foreach (var campo in errors.EnumerateObject())
                    foreach (var m in campo.Value.EnumerateArray())
                        msgs.Add(m.GetString() ?? "");
                if (msgs.Count > 0) return string.Join("\n", msgs);
            }

            if (root.TryGetProperty("mensaje", out var mensaje))
                return mensaje.GetString() ?? body;

            if (root.TryGetProperty("title", out var title))
                return title.GetString() ?? body;
        }
        catch { /* cuerpo no-JSON */ }

        return $"Error {(int)resp.StatusCode}: {body}";
    }
}
