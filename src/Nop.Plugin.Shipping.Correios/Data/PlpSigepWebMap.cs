using Nop.Data.Mapping;
using Nop.Plugin.Shipping.Correios.Domain;

namespace Nop.Plugin.Shipping.Correios.Data
{
    public class PlpSigepWebMap : NopEntityTypeConfiguration<PlpSigepWeb>
    {
        public PlpSigepWebMap()
        {
            this.ToTable("PlpSigepWeb");
            this.HasKey(x => x.Id);
        }
    }
}
