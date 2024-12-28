using Nop.Data.Mapping;
using Nop.Plugin.Shipping.Correios.Domain;

namespace Nop.Plugin.Shipping.Correios.Data
{
    public class ValorDeclaradoMap : NopEntityTypeConfiguration<ValorDeclarado>
    {
        public ValorDeclaradoMap()
        {
            this.ToTable("ValorDeclarado");
            this.HasKey(x => x.Id);

            this.HasRequired(o => o.ServicoCorreios)
                .WithRequiredPrincipal(servico => servico.ValorDeclarado);

        }

    }
}
