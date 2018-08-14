using Nop.Data.Mapping;
using Nop.Plugin.Shipping.Correios.Domain;

namespace Nop.Plugin.Shipping.Correios.Data
{
    public class PlpSigepWebShipmentMap : NopEntityTypeConfiguration<PlpSigepWebShipment>
    {
        public PlpSigepWebShipmentMap()
        {
            this.ToTable("PlpSigepWeb_Shipment");
            this.HasKey(x => x.Id);

            this.HasRequired(si => si.PlpSigepWeb)
                .WithMany(s => s.PlpSigepWebShipments)
                .HasForeignKey(si => si.PlpSigepWebId);
        }
    }
}
