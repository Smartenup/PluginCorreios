using Nop.Data.Mapping;
using Nop.Plugin.Shipping.Correios.Domain;

namespace Nop.Plugin.Shipping.Correios.Data
{
    public class GrandeFormatoMap : NopEntityTypeConfiguration<GrandeFormato>
    {
        public GrandeFormatoMap()
        {
            this.ToTable("GrandesFormatos");
            this.HasKey(x => x.Id);

        }

    }
}
