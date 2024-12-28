using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Preco
{
    public class PrecoResponse
    {
        [JsonProperty("coProduto")]
        public string CoProduto { get; set; }

        [JsonProperty("pcBase")]
        public string PcBase { get; set; }

        [JsonProperty("pcBaseGeral")]
        public string PcBaseGeral { get; set; }

        [JsonProperty("peVariacao")]
        public string PeVariacao { get; set; }

        [JsonProperty("pcReferencia")]
        public string PcReferencia { get; set; }

        [JsonProperty("vlBaseCalculoImposto")]
        public string VlBaseCalculoImposto { get; set; }

        [JsonProperty("nuRequisicao")]
        public string NuRequisicao { get; set; }

        [JsonProperty("inPesoCubico")]
        public string InPesoCubico { get; set; }

        [JsonProperty("psCobrado")]
        public string PsCobrado { get; set; }

        [JsonProperty("servicoAdicional")]
        public List<ServicoAdicionalResponse> ServicoAdicional { get; set; }

        [JsonProperty("peAdValorem")]
        public string PeAdValorem { get; set; }

        [JsonProperty("vlSeguroAutomatico")]
        public string VlSeguroAutomatico { get; set; }

        [JsonProperty("qtAdicional")]
        public string QtAdicional { get; set; }

        [JsonProperty("pcFaixa")]
        public string PcFaixa { get; set; }

        [JsonProperty("pcFaixaVariacao")]
        public string PcFaixaVariacao { get; set; }

        [JsonProperty("pcProduto")]
        public string PcProduto { get; set; }

        [JsonProperty("pcTotalServicosAdicionais")]
        public string PcTotalServicosAdicionais { get; set; }

        [JsonProperty("pcFinal")]
        public string PcFinal { get; set; }

        [JsonProperty("txErro")]
        public string textoErro { get; set; }
    }

    public class ServicoAdicionalResponse
    {
        [JsonProperty("coServAdicional")]
        public string CoServAdicional { get; set; }

        [JsonProperty("tpServAdicional")]
        public string TpServAdicional { get; set; }

        [JsonProperty("pcServicoAdicional")]
        public string PcServicoAdicional { get; set; }
    }


}
