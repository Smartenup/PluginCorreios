using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Services.Events;
using System;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public class ShipmentUpdatedEventConsumer : IConsumer<EntityUpdated<Shipment>>
    {
        private readonly ISigepWebService _sigepWebService;
        public ShipmentUpdatedEventConsumer(ISigepWebService sigepWebService)
        {
            _sigepWebService = sigepWebService;
        }

        public void HandleEvent(EntityUpdated<Shipment> eventMessage)
        {
            //Caso se altere o tracking number do shipment nop 
            //que estava associada o shipment plp, 
            //devesse excluir a associação se a plp estiver aberta

            Shipment shipment = eventMessage.Entity;

            PlpSigepWebShipment shipmentPlp = _sigepWebService.GetPlpShipment(shipment);


            if (shipmentPlp != null)
            {
                if (shipmentPlp.PlpSigepWeb.PlpStatusId != (int)PlpSigepWebStatus.Aberta )
                {
                    return;
                }

                if (!shipment.TrackingNumber.Equals(shipmentPlp.Etiqueta.CodigoEtiquetaComVerificador, StringComparison.InvariantCultureIgnoreCase))
                {
                    shipmentPlp.Etiqueta.CodigoUtilizado = false;

                    _sigepWebService.UpdateEtiqueta(shipmentPlp.Etiqueta);

                    _sigepWebService.DeletePlpShipment(shipmentPlp);
                }
            }

        }
    }
}
