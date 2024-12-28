using Nop.Core;

namespace Nop.Plugin.Shipping.Correios.Domain
{
    public class ValorDeclarado : BaseEntity
    {
        public int ServicoCorreiosId { get; set; }

        public virtual ServicoCorreios ServicoCorreios { get; set; }

        public string CodigoCorreiosValorDeclarado { get; set; }

        public decimal MinimoValorDeclarado { get; set; }

        public decimal MaximoValorDeclarado { get; set; }
    }
}
