using Nop.Data.Mapping;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Plugin.Shipping.Correios.wsAtendeClienteService;

namespace Nop.Plugin.Shipping.Correios.Data
{
    public class ServicoCorreiosMap : NopEntityTypeConfiguration<ServicoCorreios>
    {
        public ServicoCorreiosMap()
        {
            this.ToTable("ServicoCorreios");
            this.HasKey(x => x.Id);

            this.HasRequired(servicoCorreios => servicoCorreios.ValorDeclarado)
                .WithRequiredDependent(valorDeclarado => valorDeclarado.ServicoCorreios);
        }

    }
}
