using Nop.Core;
using System;

namespace Nop.Plugin.Shipping.Correios.Domain
{
    public class PlpSigepWebShipment : BaseEntity
    {
        public int EtiquetaId { get; set; }

        public int PlpSigepWebId { get; set; }

        public int ShipmentId { get; set; }        

        public int OrderId { get; set; }

        public bool Deleted { get; set; }

        public decimal ValorDeclarado { get; set; }

        public decimal PesoEstimado { get; set; }

        public decimal PesoEfetivado { get; set; }

        public decimal ValorEnvioEstimado { get; set; }

        public decimal ValorEnvioEfetivado { get; set; }

        public DateTime? DataPostagemSara { get; set; }

        public DateTime? DataCaptacaoSara { get; set; }

        public int? StatusProcessamentoSara { get; set; }

        public long? NumeroComprovantePostagem { get; set; }

        public int? QuantidadeDiasUteis { get; set; }

        public DateTime? DataPrevistaEntrega { get; set; }

        public int? ControleEnvioStatusId { get; set; }

        public virtual PlpSigepWeb PlpSigepWeb { get; set; }

        public virtual PlpSigepWebEtiqueta Etiqueta { get; set; }
    }
}
