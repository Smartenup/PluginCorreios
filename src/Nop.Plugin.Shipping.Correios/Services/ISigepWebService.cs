using Nop.Core;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Domain;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public interface ISigepWebService
    {
        void DeletePlp(PlpSigepWeb plpSigepWeb);

        void InsertPlp(PlpSigepWeb plpSigepWeb);

        void UpdatePlp(PlpSigepWeb plpSigepWeb);

        void DeletePlpShipment(PlpSigepWebShipment plpSigepWebShipment);

        void InsertPlpShipment(PlpSigepWebShipment plpSigepWebShipment);

        PlpSigepWebShipment GetPlpShipment(int Id);

        PlpSigepWebShipment GetPlpShipment(Shipment shipment);

        PlpSigepWeb GetPlp(PlpSigepWebStatus status);

        PlpSigepWeb GetPlp(int Id);

        IList<PlpSigepWebShipment> GetShipmentsByIds(int[] plpSigepWebShimentId);

        void UpdateEtiqueta(PlpSigepWebEtiqueta etiqueta);

        void InsertEtiqueta(PlpSigepWebEtiqueta etiqueta);

        IPagedList<PlpSigepWeb> ProcurarPlp(PlpSigepWebStatus status, DateTime? dataFechamentoInicial = null, 
            DateTime? dataFechamentoFinal = null, int pedidoId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue);

    }
}
