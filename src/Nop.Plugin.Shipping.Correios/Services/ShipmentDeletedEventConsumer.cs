using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Services.Events;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public class ShipmentDeletedEventConsumer : IConsumer<EntityDeleted<Shipment>>
    {
        private readonly ISigepWebService _sigepWebService;
        public ShipmentDeletedEventConsumer(ISigepWebService sigepWebService)
        {
            _sigepWebService = sigepWebService;
        }

        public void HandleEvent(EntityDeleted<Shipment> eventMessage)
        {
            //Caso se exclua o shipment nop 
            //que estava associada o shipment plp, 
            //devesse excluir a associação plp se a plp estiver aberta

            Shipment shipment = eventMessage.Entity;

            PlpSigepWebShipment  shipmentPlp =_sigepWebService.GetPlpShipment(shipment);

            if (shipmentPlp != null)
            {
                if (shipmentPlp.PlpSigepWeb.PlpStatusId != (int)PlpSigepWebStatus.Aberta)
                {
                    return;
                }

                shipmentPlp.Etiqueta.CodigoUtilizado = false;

                _sigepWebService.UpdateEtiqueta(shipmentPlp.Etiqueta);
                
                _sigepWebService.DeletePlpShipment(shipmentPlp);
            }
        }
    }
}
