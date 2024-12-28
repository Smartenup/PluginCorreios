using Nop.Data.Mapping;
using Nop.Plugin.Shipping.Correios.Domain;

namespace Nop.Plugin.Shipping.Correios.Data
{
    public class EmbalagemServicoCorreiosMap : NopEntityTypeConfiguration<EmbalagemServicoCorreios>
    {
        public EmbalagemServicoCorreiosMap()
        {
            this.ToTable("EmbalagemServicoCorreios");
            this.HasKey(x => x.Id);
            
            this.HasRequired(o => o.ServicoCorreios)
                .WithMany()
                .HasForeignKey(o => o.ServicoCorreiosId)
                .WillCascadeOnDelete(false);

            this.HasRequired(o => o.Embalagem)
                .WithMany()
                .HasForeignKey(o => o.EmbalagemId)
                .WillCascadeOnDelete(false);



        }

    }
}
