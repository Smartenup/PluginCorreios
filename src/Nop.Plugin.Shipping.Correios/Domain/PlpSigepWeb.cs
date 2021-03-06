﻿using Nop.Core;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios.Domain
{
    public class PlpSigepWeb : BaseEntity
    {
        private ICollection<PlpSigepWebShipment> _plpSigepWebShipments;

        public int PlpStatusId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime? FechamentoOnUtc { get; set; }

        public bool Deleted { get; set; }

        public string XmlPLP { get; set; }

        public int CustomerId { get; set; }

        public long? PlpSigepWebCorreiosId { get; set; }

        public int? PlpSigepWebControleEnvioStatusId { get; set; }

        /// <summary>
        /// Gets or sets shipments
        /// </summary>
        public virtual ICollection<PlpSigepWebShipment> PlpSigepWebShipments
        {
            get { return _plpSigepWebShipments ?? (_plpSigepWebShipments = new List<PlpSigepWebShipment>()); }
            protected set { _plpSigepWebShipments = value; }
        }
    }

}
