using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        if (string.IsNullOrWhiteSpace(BanxicoToken) ||BanxicoToken == "PEGA_AQUI_TU_TOKEN")
        {
            await DisplayAlert(
                "Token requerido",
                "Coloca tu token de Banxico en MainPage.xaml.cs.",
                "Aceptar");

            return;
        }

        ConsultarButton.IsEnabled = false;
        ResultadoLabel.Text = "Consultando Banxico...";

        try
        {
            var respuesta = await _banxicoService.ConsultarAsync(
                fechaInicial.Value.Date,
                fechaFinal.Value.Date,
                BanxicoToken);

            if (respuesta.Bmx.Series.Count == 0)
            {
                ResultadoLabel.Text = "No se encontraron series.";
                return;
            }

            var serie = respuesta.Bmx.Series[0];
            var texto = new StringBuilder();
            int cantidad = 0;

            texto.AppendLine(serie.Titulo);
            texto.AppendLine();

            if (serie.Datos != null)
            {
                foreach (var dato in serie.Datos)
                {
                    if (string.IsNullOrWhiteSpace(dato.Dato) ||
                        dato.Dato == "N/E")
                    {
                        continue;
                    }

                    texto.AppendLine(
                        $"{dato.Fecha}: {dato.Dato} MXN por USD");

                    cantidad++;
                }
            }

            ResultadoLabel.Text = cantidad > 0
                ? texto.ToString()
                : "No hay datos disponibles para el periodo seleccionado.";
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