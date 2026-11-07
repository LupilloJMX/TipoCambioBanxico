using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace TipoCambioBanxico;

public partial class MainPage : ContentPage
{
    private readonly BanxicoService _banxicoService = new();

    private const string BanxicoToken = "a027df81d8f41f35cadfa13e8feac7a01fc4474f26e973d8fc28b61fd9653327";

    public MainPage()
    {
        InitializeComponent();
    }

    private async void ConsultarButton_Clicked(object sender, EventArgs e)
    {
        DateTime? fechaInicial = FechaInicialPicker.Date;
        DateTime? fechaFinal = FechaFinalPicker.Date;

        if (!fechaInicial.HasValue || !fechaFinal.HasValue)
        {
            await DisplayAlert(
                "Fechas requeridas",
                "Selecciona la fecha inicial y la fecha final.",
                "Aceptar");

            return;
        }

        if (fechaInicial.Value.Date > fechaFinal.Value.Date)
        {
            await DisplayAlert(
                "Revisa las fechas",
                "La fecha inicial no puede ser posterior a la fecha final.",
                "Aceptar");

            return;
        }

        if (string.IsNullOrWhiteSpace(BanxicoToken) ||BanxicoToken == "TOKEN")
        {
            await DisplayAlert(
                "Token requerido",
                "Coloca tu token de Banxico en MainPage.xaml.cs.",
                "Aceptar");

            return;
        }

        string? idSerie = SeriePicker.SelectedIndex switch
        {
            0 => "SF43718",
            1 => "SF60653",
            2 => "SF46410",
            _ => null
        };

        if (idSerie == null)
        {
            await DisplayAlert(
                "Serie requerida",
                "Selecciona una serie.",
                "Aceptar");

            return;
        }

        ConsultarButton.IsEnabled = false;
        ResultadosCollection.ItemsSource = null;
        ResultadoLabel.Text = "Consultando Banxico...";

        try
        {
            var respuesta = await _banxicoService.ConsultarAsync(
        fechaInicial.Value.Date,
        fechaFinal.Value.Date,
        idSerie,
        BanxicoToken);

            if (respuesta.Bmx.Series.Count == 0)
            {
                ResultadoLabel.Text = "No se encontraron series.";
                return;
            }

            var serie = respuesta.Bmx.Series[0];

            var datos = serie.Datos?
                .Where(d => !string.IsNullOrWhiteSpace(d.Dato)
                            && d.Dato != "N/E")
                .ToList();

            if (datos == null || datos.Count == 0)
            {
                ResultadoLabel.Text =
                    "No hay datos disponibles para el periodo seleccionado.";

                return;
            }

            ResultadosCollection.ItemsSource = datos;

            ResultadoLabel.Text =
                $"Tipo de cambio {idSerie} · MXN por USD\n" +
                $"Registros encontrados: {datos.Count}";

        }
        catch (HttpRequestException ex)
        {
            ResultadoLabel.Text =
                $"No se pudo consultar Banxico. Revisa tu conexión y el token.\n{ex.Message}";
        }
        catch (OperationCanceledException)
        {
            ResultadoLabel.Text =
                "La consulta tardó demasiado. Inténtalo nuevamente.";
        }
        catch (Exception ex)
        {
            ResultadoLabel.Text =
                $"Ocurrió un error al procesar la respuesta: {ex.Message}";
        }
        finally
        {
            ConsultarButton.IsEnabled = true;
        }
    }
}