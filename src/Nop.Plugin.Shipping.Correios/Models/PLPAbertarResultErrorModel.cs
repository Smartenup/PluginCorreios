using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Shipping.Correios.Models
{
    public class PLPAbertaResultErrorModel : BaseNopModel
    {
        public int CodigoPedidoErro { get; set; }
        public string MensagemErro { get; set; }
    }    
}
