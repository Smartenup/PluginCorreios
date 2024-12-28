using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Prazo
{
    public class PrazosRequest
    {
        public string idLote { get; set; }
        public List<ParametrosPrazo> parametrosPrazo { get; set; }

    }

    public class ParametrosPrazo
    {
        public string cepDestino { get; set; }
        public string cepOrigem { get; set; }
        public string coProduto { get; set; }
        public string nuRequisicao { get; set; }
        public string dtEvento { get; set; }
        public string dataPostagem { get; set; }
    }
}
