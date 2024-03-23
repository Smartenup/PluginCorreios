using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Rastro
{
    public class Rastro
    {
        public string Versao { get; set; }
        public int Quantidade { get; set; }
        public List<Objeto> Objetos { get; set; }
        public string TipoResultado { get; set; }
    }
}
