using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Domain;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public interface ISigepWebPlpService
    {
        PlpSigepWeb ObterPlpEmAberto();

        wsAtendeClienteService.clienteERP ObterClienteSigepWEB();

        void VerificarStatusCartaoPostegem();

        string ObterEndPointAtendeCliente();

        List<PlpSigepWebEtiqueta> SolicitaEtiquetas(IList<Order> lstPedidos, wsAtendeClienteService.clienteERP cliente);

        PlpSigepWebEtiqueta ObterProximaEtiquetaDisponivel(List<PlpSigepWebEtiqueta> lstEtiquetas, string shippingMethod);

        Shipment CriarShipmentNop(Order order, string trackingNumber);

        PlpSigepWeb FecharPlp();

        PlpSigepWeb ObterPlp(int Id);

        PlpSigepWebEtiqueta ObterProximaEtiquetaDisponivel(string nomeServicoPublico);        
    }
}
