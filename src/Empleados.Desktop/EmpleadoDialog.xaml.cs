using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Empleados.Api.Dtos;
using Empleados.Api.Models;

namespace Empleados.Desktop;

public partial class EmpleadoDialog : Window
{
    /// <summary>DTO listo para enviar a la API. Null si el usuario cancela.</summary>
    public EmpleadoInput? Resultado { get; private set; }

    public EmpleadoDialog(Empleado? existente = null)
    {
        InitializeComponent();

        foreach (var d in Enum.GetValues<Departamento>())
            CmbDepartamento.Items.Add(d.ToString());
        CmbDepartamento.SelectedIndex = 0;

        AplicarSelectoresDinamicos();

        if (existente is not null)
        {
            Title = "Editar empleado";
            TituloLabel.Text = $"Editar {existente.NumeroEmpleado}";
            TxtNombre.Text = existente.Nombre;
            TxtApellidos.Text = existente.Apellidos;
            TxtDni.Text = existente.Dni;
            TxtEmail.Text = existente.Email;
            CmbDepartamento.SelectedIndex = (int)existente.Departamento;
            TxtPuesto.Text = existente.Puesto;
            TxtSalario.Text = existente.Salario.ToString("0.##", CultureInfo.InvariantCulture);
            ChkActivo.IsChecked = existente.Activo;
        }
    }

    // BARRERA 1: AutomationId aleatorio por sesion tambien en el formulario.
    private void AplicarSelectoresDinamicos()
    {
        SelectorObfuscator.AleatorizarTodos(
            (TxtNombre, "txtNombre"), (TxtApellidos, "txtApellidos"),
            (TxtDni, "txtDni"), (TxtEmail, "txtEmail"),
            (CmbDepartamento, "cmbDepto"), (TxtSalario, "txtSalario"),
            (TxtPuesto, "txtPuesto"), (ChkActivo, "chkActivo"),
            (BtnGuardar, "btnGuardar"), (BtnCancelar, "btnCancelar"));
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        // Validacion minima en cliente; la API hace la validacion completa (DNI, duplicados, rangos).
        if (string.IsNullOrWhiteSpace(TxtNombre.Text) ||
            string.IsNullOrWhiteSpace(TxtApellidos.Text) ||
            string.IsNullOrWhiteSpace(TxtDni.Text) ||
            string.IsNullOrWhiteSpace(TxtEmail.Text) ||
            string.IsNullOrWhiteSpace(TxtPuesto.Text))
        {
            MessageBox.Show("Complete todos los campos obligatorios.",
                "Faltan datos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(TxtSalario.Text.Replace(".", "").Replace(",", "."),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var salario))
        {
            MessageBox.Show("El salario debe ser un numero.",
                "Dato no valido", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Resultado = new EmpleadoInput
        {
            Nombre = TxtNombre.Text.Trim(),
            Apellidos = TxtApellidos.Text.Trim(),
            Dni = TxtDni.Text.Trim(),
            Email = TxtEmail.Text.Trim(),
            Departamento = (Departamento)CmbDepartamento.SelectedIndex,
            Puesto = TxtPuesto.Text.Trim(),
            Salario = salario,
            Activo = ChkActivo.IsChecked == true
        };

        DialogResult = true;
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
