using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Preco
{
    public class PrecosRequest
    {
        [JsonProperty("idLote")]
        public string IdLote { get; set; }

        [JsonProperty("parametrosProduto")]
        public List<ParametrosProduto> ParametrosProduto { get; set; }

    }

    public class ParametrosProduto
    {
        [JsonProperty("coProduto")]
        public string CoProduto { get; set; }

        [JsonProperty("nuRequisicao")]
        public string NuRequisicao { get; set; }

        [JsonProperty("cepOrigem")]
        public string CepOrigem { get; set; }

        [JsonProperty("psObjeto")]
        public string PsObjeto { get; set; }

        [JsonProperty("tpObjeto")]
        public string TpObjeto { get; set; }

        [JsonProperty("comprimento")]
        public string Comprimento { get; set; }

        [JsonProperty("largura")]
        public string Largura { get; set; }

        [JsonProperty("altura")]
        public string Altura { get; set; }

        [JsonProperty("servicosAdicionais")]
        public List<ServicosAdicionalRequest> ServicosAdicionais { get; set; }

        [JsonProperty("vlDeclarado")]
        public string VlDeclarado { get; set; }

        [JsonProperty("dtEvento")]
        public string DtEvento { get; set; }

        [JsonProperty("cepDestino")]
        public string CepDestino { get; set; }
    }

    public class ServicosAdicionalRequest
    {
        [JsonProperty("coServAdicional")]
        public string CoServAdicional { get; set; }
    }
}
