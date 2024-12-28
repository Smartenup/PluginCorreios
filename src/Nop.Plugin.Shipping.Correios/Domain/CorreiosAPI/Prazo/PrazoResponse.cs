using System;

namespace Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Prazo
{
    public class PrazoResponse
    {
        public string coProduto { get; set; }
        public string nuRequisicao { get; set; }
        public int prazoEntrega { get; set; }
        public DateTime dataMaxima { get; set; }
        public string entregaDomiciliar { get; set; }
        public string entregaSabado { get; set; }
        public string entregaDomingo { get; set; }
        public string txErro { get; set; }
    }


}
