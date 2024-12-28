using Nop.Core;

namespace Nop.Plugin.Shipping.Correios.Domain
{
    public class Embalagem : BaseEntity
    {
        public string CodigoEmbalagem { get; set; }

        public string DescricaoEmbalagem { get; set; }
    }
}
