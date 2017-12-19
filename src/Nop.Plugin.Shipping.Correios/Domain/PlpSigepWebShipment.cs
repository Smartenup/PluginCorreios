using Nop.Core;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Shipping.Correios.Domain
{
    public class PlpSigepWebShipment : BaseEntity
    {
        public int EtiquetaId { get; set; }

        public int PlpSigepWebId { get; set; }

        public int ShipmentId { get; set; }        

        public bool Deleted { get; set; }

        public decimal ValorDeclarado { get; set; }

        public decimal PesoEstimado { get; set; }

        public decimal PesoEfetivado { get; set; }

        public decimal ValorEnvioEstimado { get; set; }

        public decimal ValorEnvioEfetivado { get; set; }

        public virtual PlpSigepWeb PlpSigepWeb { get; set; }

        public virtual PlpSigepWebEtiqueta Etiqueta { get; set; }
    }
}
