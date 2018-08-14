using Nop.Data.Mapping;
using Nop.Plugin.Shipping.Correios.Domain;

namespace Nop.Plugin.Shipping.Correios.Data
{
    public class PlpSigepWebEtiquetaMap : NopEntityTypeConfiguration<PlpSigepWebEtiqueta>
    {
        public PlpSigepWebEtiquetaMap()
        {
            this.ToTable("PlpSigepWebEtiqueta");
            this.HasKey(x => x.Id);
        }
    }
}
