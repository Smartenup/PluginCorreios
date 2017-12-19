using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Shipping.Correios.Domain
{
    public class PlpSigepWebEtiqueta : BaseEntity
    {
        public string CodigoEtiquetaSemVerificador { get; set; }

        public string CodigoEtiquetaComVerificador { get; set; }

        public string CodigoServico { get; set; }

        public bool CodigoUtilizado { get; set; } 
    }
}
