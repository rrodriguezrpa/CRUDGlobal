using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Empleados.Api.Dtos;
using Empleados.Api.Models;

namespace Empleados.Desktop;

public partial class MainWindow : Window
{
    private readonly ApiClient _api;
    private readonly LoginResponse _usuario;
    private readonly Random _rng = new();

    private int _page = 1;
    private int _pageSize = 5;
    private int _totalPages = 1;

    public MainWindow(ApiClient api, LoginResponse usuario)
    {
        InitializeComponent();
        _api = api;
        _usuario = usuario;

        SessionLabel.Text = $"{_usuario.NombreCompleto} ({_usuario.Rol})  ·  Sesion: {SelectorObfuscator.SessionToken}";
        // BARRERA 1: titulo de ventana variable por sesion (rompe selectores por title).
        Title = $"Gestion de Empleados [{SelectorObfuscator.SessionToken}]";

        InicializarFiltroDepartamento();
        AplicarSelectoresDinamicos();

        Loaded += async (_, _) => await CargarAsync();
    }

    private void InicializarFiltroDepartamento()
    {
        DeptFilter.Items.Add("Todos");
        foreach (var d in Enum.GetValues<Departamento>())
            DeptFilter.Items.Add(d.ToString());
        DeptFilter.SelectedIndex = 0;
    }

    // BARRERA 1: AutomationId aleatorio en cada control interactivo.
    private void AplicarSelectoresDinamicos()
    {
        SelectorObfuscator.AleatorizarTodos(
            (SearchBox, "txtBuscar"), (DeptFilter, "cmbDepto"),
            (BtnBuscar, "btnBuscar"), (BtnRefrescar, "btnRefrescar"),
            (BtnNuevo, "btnNuevo"), (BtnEditar, "btnEditar"), (BtnEliminar, "btnEliminar"),
            (BtnPrev, "btnPrev"), (BtnNext, "btnNext"), (Grid, "gridEmpleados"));
    }

    private Departamento? DepartamentoSeleccionado()
        => DeptFilter.SelectedIndex <= 0 ? null : (Departamento)(DeptFilter.SelectedIndex - 1);

    private async Task CargarAsync()
    {
        var res = await _api.ListarAsync(
            SearchBox.Text, DepartamentoSeleccionado(), _page, _pageSize);

        if (!res.Ok || res.Value is null)
        {
            MessageBox.Show($"No se pudo cargar la lista.\n\n{res.Error}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var data = res.Value;
        Grid.ItemsSource = data.Items;
        _totalPages = Math.Max(1, data.TotalPages);
        _page = Math.Min(_page, _totalPages);

        TotalLabel.Text = $"{data.TotalItems} empleados en total";
        PageLabel.Text = $"Pagina {_page} de {_totalPages}";
        BtnPrev.IsEnabled = _page > 1;
        BtnNext.IsEnabled = _page < _totalPages;
    }

    // ---- busqueda / refresco ----

    private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
    {
        _page = 1;
        await CargarAsync();
    }

    private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { _page = 1; await CargarAsync(); }
    }

    private async void BtnRefrescar_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Clear();
        DeptFilter.SelectedIndex = 0;
        _page = 1;
        await CargarAsync();
    }

    // ---- paginacion ----

    private async void BtnPrev_Click(object sender, RoutedEventArgs e)
    {
        if (_page > 1) { _page--; await CargarAsync(); }
    }

    private async void BtnNext_Click(object sender, RoutedEventArgs e)
    {
        if (_page < _totalPages) { _page++; await CargarAsync(); }
    }

    private async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        if (PageSizeCombo.SelectedItem is ComboBoxItem item && int.TryParse(item.Content?.ToString(), out var size))
        {
            _pageSize = size;
            _page = 1;
            await CargarAsync();
        }
    }

    // ---- CRUD ----

    private async void BtnNuevo_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new EmpleadoDialog { Owner = this };
        if (dlg.ShowDialog() != true) return;

        var res = await _api.CrearAsync(dlg.Resultado!);
        if (res.Ok)
        {
            ConfirmDialog.Aviso(this, "Empleado creado correctamente.");
            await CargarAsync();
        }
        else
        {
            MessageBox.Show(res.Error, "No se pudo crear", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnEditar_Click(object sender, RoutedEventArgs e) => await EditarSeleccionadoAsync();

    private async void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => await EditarSeleccionadoAsync();

    private async Task EditarSeleccionadoAsync()
    {
        if (Grid.SelectedItem is not Empleado emp)
        {
            ConfirmDialog.Aviso(this, "Seleccione un empleado de la tabla primero.");
            return;
        }

        var dlg = new EmpleadoDialog(emp) { Owner = this };
        if (dlg.ShowDialog() != true) return;

        var res = await _api.ActualizarAsync(emp.Id, dlg.Resultado!);
        if (res.Ok)
        {
            ConfirmDialog.Aviso(this, "Cambios guardados.");
            await CargarAsync();
        }
        else
        {
            MessageBox.Show(res.Error, "No se pudo actualizar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Empleado emp)
        {
            ConfirmDialog.Aviso(this, "Seleccione un empleado de la tabla primero.");
            return;
        }

        // BARRERA 2: dialogo modal de confirmacion con botones en posicion aleatoria
        // y, a veces, una segunda confirmacion. Rompe clicks por coordenadas/orden fijo.
        var confirmado = ConfirmDialog.Confirmar(this,
            $"Va a eliminar a {emp.Nombre} {emp.Apellidos} ({emp.NumeroEmpleado}).\n\nEsta accion no se puede deshacer.");
        if (!confirmado) return;

        if (_rng.Next(2) == 0)
        {
            var doble = ConfirmDialog.Confirmar(this,
                "Confirme de nuevo: se eliminara el registro de forma permanente.");
            if (!doble) return;
        }

        var res = await _api.EliminarAsync(emp.Id);
        if (res.Ok)
        {
            ConfirmDialog.Aviso(this, "Empleado eliminado.");
            await CargarAsync();
        }
        else
        {
            MessageBox.Show(res.Error, "No se pudo eliminar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
