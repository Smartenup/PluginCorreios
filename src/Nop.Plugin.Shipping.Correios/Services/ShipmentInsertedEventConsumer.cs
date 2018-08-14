using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Services.Events;
using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public class ShipmentInsertedEventConsumer : IConsumer<EntityInserted<Shipment>>
    {
        private readonly ISigepWebPlpService _sigepWebPlpService;
        private readonly ISigepWebService _sigepWebService;
        private readonly IShipmentService _shipmentService;

        public ShipmentInsertedEventConsumer(ISigepWebPlpService sigepWebPlpService,
            ISigepWebService sigepWebService,
            IShipmentService shipmentService)
        {
            _sigepWebPlpService = sigepWebPlpService;
            _sigepWebService = sigepWebService;
            _shipmentService = shipmentService;
        }

        public void HandleEvent(EntityInserted<Shipment> eventMessage)
        {
            Shipment shipment = eventMessage.Entity;

            if (string.IsNullOrWhiteSpace(shipment.TrackingNumber))
            {
                ///Obtem a próxima etiqueta para o serviço selecionado
                PlpSigepWebEtiqueta etiqueta = _sigepWebPlpService.ObterProximaEtiquetaDisponivel(shipment.Order.ShippingMethod);

                //Atualiza o numero de rastreio do pedido
                shipment.TrackingNumber = etiqueta.CodigoEtiquetaComVerificador;
                _shipmentService.UpdateShipment(shipment);


                PlpSigepWeb plpAberta = _sigepWebPlpService.ObterPlpEmAberto();

                ///Cria o shipmentPLP
                var plpSigepWebShipment = new PlpSigepWebShipment();

                plpSigepWebShipment.Deleted = false;
                plpSigepWebShipment.EtiquetaId = etiqueta.Id;
                plpSigepWebShipment.PlpSigepWebId = plpAberta.Id;
                plpSigepWebShipment.ShipmentId = shipment.Id;
                plpSigepWebShipment.OrderId = shipment.OrderId;
                plpSigepWebShipment.PesoEstimado = shipment.TotalWeight.HasValue ? shipment.TotalWeight.Value : 0;
                plpSigepWebShipment.ValorEnvioEstimado = shipment.Order.OrderShippingInclTax;
                plpSigepWebShipment.ValorDeclarado = CorreiosServices.ObterValorDeclarado(shipment.Order.OrderSubtotalInclTax, etiqueta.CodigoServico);

                plpAberta.PlpSigepWebShipments.Add(plpSigepWebShipment);

                _sigepWebService.UpdatePlp(plpAberta);                

            }
        }
    }
}
