using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Shipping.Correios.Domain
{
    public enum PlpSigepWebStatus
    {
        /// <summary>
        /// Aberta
        /// </summary>
        Aberta = 10,
        /// <summary>
        /// Fechada
        /// </summary>
        Fechada = 20,
        /// <summary>
        /// Cancelada
        /// </summary>
        Cancelada = 40
    }
}
