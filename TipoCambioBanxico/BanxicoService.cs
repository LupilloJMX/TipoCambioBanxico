using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace TipoCambioBanxico
{
    public class BanxicoService
    {
        private static readonly HttpClient Cliente = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public async Task<BanxicoResponse> ConsultarAsync(
            DateTime fechaInicial,
            DateTime fechaFinal,
             string idSerie,
            string token)
        {
            if (fechaInicial.Date > fechaFinal.Date)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser posterior a la fecha final.");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Falta el token de Banxico.");
            }

            string inicio = fechaInicial.ToString(
                "yyyy-MM-dd", CultureInfo.InvariantCulture);

            string fin = fechaFinal.ToString(
                "yyyy-MM-dd", CultureInfo.InvariantCulture);

            // Serie del tipo de cambio FIX.
            string url =
                "https://www.banxico.org.mx/SieAPIRest/service/v1/" +
                $"series/{idSerie}/datos/{inicio}/{fin}";

            using var solicitud = new HttpRequestMessage(HttpMethod.Get, url);

            solicitud.Headers.Accept.ParseAdd("application/json");
            solicitud.Headers.Add("Bmx-Token", token.Trim());

            using var respuesta = await Cliente.SendAsync(solicitud);

            respuesta.EnsureSuccessStatusCode();

            using var contenido = await respuesta.Content.ReadAsStreamAsync();

            var resultado =
                await JsonSerializer.DeserializeAsync<BanxicoResponse>(contenido);

            if (resultado?.Bmx?.Series == null)
            {
                throw new InvalidOperationException(
                    "Banxico devolvió una respuesta no válida.");
            }

            return resultado;
        }
    }

}