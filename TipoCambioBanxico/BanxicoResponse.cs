using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace TipoCambioBanxico
{
    public class BanxicoResponse
    {
        [JsonPropertyName("bmx")]
        public BanxicoContenido Bmx { get; set; } = new();
    }

    public class BanxicoContenido
    {
        [JsonPropertyName("series")]
        public List<BanxicoSerie> Series { get; set; } = new();
    }

    public class BanxicoSerie
    {
        [JsonPropertyName("idSerie")]
        public string IdSerie { get; set; } = string.Empty;

        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [JsonPropertyName("datos")]
        public List<BanxicoDato> Datos { get; set; } = new();
    }

    public class BanxicoDato
    {
        [JsonPropertyName("fecha")]
        public string Fecha { get; set; } = string.Empty;

        [JsonPropertyName("dato")]
        public string Dato { get; set; } = string.Empty;
    }
}
