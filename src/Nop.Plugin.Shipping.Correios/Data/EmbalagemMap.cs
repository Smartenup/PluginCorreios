using Nop.Data.Mapping;
using Nop.Plugin.Shipping.Correios.Domain;

namespace Nop.Plugin.Shipping.Correios.Data
{
    public class EmbalagemMap : NopEntityTypeConfiguration<Embalagem>
    {

        public EmbalagemMap()
        {
            this.ToTable("Embalagem");
            this.HasKey(x => x.Id);
        }

    }
}
