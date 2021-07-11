using Nop.Core;
using System;

namespace Nop.Plugin.Shipping.Correios.Domain
{
    public class ShippingCorreiosConfiguracaoServicos : BaseEntity
    {
        public string CodigoServicoEstimativa { get; set; }

        public string CodigoServicoEnvioPLP { get; set; }

        public string DescricaoServicoPedido { get; set; }
        
    }
}
