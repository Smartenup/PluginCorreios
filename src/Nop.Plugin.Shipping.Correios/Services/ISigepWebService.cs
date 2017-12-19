using Nop.Plugin.Shipping.Correios.Domain;
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

        PlpSigepWeb GetPlp(PlpSigepWebStatus status);

        PlpSigepWeb GetPlp(int Id);

        IList<PlpSigepWebShipment> GetShipmentsByIds(int[] plpSigepWebShimentId);

        void UpdateEtiquetaCorreios(PlpSigepWebEtiqueta etiqueta);

        void InsertEtiquetaCorreios(PlpSigepWebEtiqueta etiqueta);

    }
}
