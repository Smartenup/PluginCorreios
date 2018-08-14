using Nop.Web.Framework.Mvc;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Shipping.Correios.Models
{
    public class PLPFechadaSearchModel : BaseNopModel
    {
        [DisplayName("Data inicio")]
        [UIHint("DateNullable")]
        public DateTime? DataInicio { get; set; }

        [DisplayName("Data final")]
        [UIHint("DateNullable")]
        public DateTime? DataFinal { get; set; }
        [DisplayName("Número do pedido")]
        
        public int NumeroPedido { get; set; }
    }
}
