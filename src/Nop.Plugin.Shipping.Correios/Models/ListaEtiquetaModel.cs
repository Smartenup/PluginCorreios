using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios.Models
{
    public class ListaEtiquetaModel : BaseNopModel
    {
        public ListaEtiquetaModel()
        {
            Etiquetas = new List<EtiquetaModel>();
        }

        public List<EtiquetaModel> Etiquetas { get; set; }
    }
}
