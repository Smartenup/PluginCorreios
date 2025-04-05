using Newtonsoft.Json;

namespace Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.CEP
{
    public class CEPResponse
    {
        [JsonProperty("cep")]
        public string Cep { get; set; }

        [JsonProperty("uf")]
        public string Uf { get; set; }

        [JsonProperty("numeroLocalidade")]
        public int? NumeroLocalidade { get; set; }

        [JsonProperty("localidade")]
        public string Localidade { get; set; }

        [JsonProperty("logradouro")]
        public string Logradouro { get; set; }

        [JsonProperty("tipoLogradouro")]
        public string TipoLogradouro { get; set; }

        [JsonProperty("nomeLogradouro")]
        public string NomeLogradouro { get; set; }

        [JsonProperty("complemento")]
        public string Complemento { get; set; }

        [JsonProperty("abreviatura")]
        public string Abreviatura { get; set; }

        [JsonProperty("bairro")]
        public string Bairro { get; set; }

        [JsonProperty("tipoCEP")]
        public int? TipoCEP { get; set; }

        [JsonProperty("cepUnidadeOperacional")]
        public string CepUnidadeOperacional { get; set; }

        [JsonProperty("lado")]
        public string Lado { get; set; }

        [JsonProperty("numeroInicial")]
        public int? NumeroInicial { get; set; }

        [JsonProperty("numeroFinal")]
        public int? NumeroFinal { get; set; }
    }
}
