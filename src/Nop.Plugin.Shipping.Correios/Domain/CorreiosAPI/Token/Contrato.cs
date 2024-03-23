using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Token
{
    public class Contrato
    {
        public string Numero { get; set; }
        public int Dr { get; set; }
        public List<int> Api { get; set; }
    }
}
