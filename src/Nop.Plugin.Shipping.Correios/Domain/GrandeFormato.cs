using Nop.Core;

namespace Nop.Plugin.Shipping.Correios.Domain
{
    public class GrandeFormato : BaseEntity
    {
        public int ServicoCorreiosId { get; set; }

        public int ServicoCorreiosIdGrandeFormato { get; set; }

        public string CodigoServicoGrandeFormatoCorreios { get; set; }
    }
}
