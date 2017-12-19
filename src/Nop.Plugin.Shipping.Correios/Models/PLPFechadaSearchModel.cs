using Nop.Web.Framework.Mvc;
using System;
using System.ComponentModel;

namespace Nop.Plugin.Shipping.Correios.Models
{
    public class PLPFechadaSearchModel : BaseNopModel
    {
        [DisplayName("Data inicio")]
        public DateTime? DataInicio { get; set; }
        [DisplayName("Data final")]
        public DateTime? DataFinal { get; set; }
        [DisplayName("Número do pedido")]
        public int NumeroPedido { get; set; }
    }
}
