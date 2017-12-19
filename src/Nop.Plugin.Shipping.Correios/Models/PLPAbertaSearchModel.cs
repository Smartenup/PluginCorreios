using Nop.Web.Framework.Mvc;
using System.ComponentModel;

namespace Nop.Plugin.Shipping.Correios.Models
{
    public class PLPAbertaSearchModel: BaseNopModel
    {
        [DisplayName("Pedidos separados por virgula (,)")]
        public string OrdersIs { get; set; }

    }
}
