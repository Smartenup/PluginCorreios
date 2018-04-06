using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Domain;
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

        /// <summary>
        /// Valida os pedidos e os retira da lista caso tenha problemas
        /// </summary>
        /// <param name="lstPedidos">Pedidos que se quer criar etiqueta</param>
        /// <param name="lstProblemas">Pedidos com problemas no endereço</param>
        /// <returns></returns>
        bool ValidarPedidosEtiqueta(IList<Order> lstPedidos, out List<KeyValuePair<Order, int>> lstProblemas);

        wsAtendeClienteService.enderecoERP BuscarEndereco(string cep);

        string SolicitaXmlPlp(long plpIdCorreios);
    }
}
