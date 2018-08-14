using Nop.Plugin.Shipping.Correios.Domain;
using System.Collections.Generic;
using System.IO;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public interface IPdfSigepWebService
    {
        void PrintEtiquetasToPdf(Stream stream, IList<PlpSigepWebShipment> sigepWebShipments);


        void PrintFechamentoToPdf(Stream stream, PlpSigepWeb plpSigepWeb);
    }
}
