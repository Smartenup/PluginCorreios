using Nop.Core;

namespace Nop.Plugin.Shipping.Correios.Domain
{
    public class EmbalagemServicoCorreios : BaseEntity
    {
        public int ServicoCorreiosId { get; set; }

        public virtual ServicoCorreios ServicoCorreios { get; set; }

        public int EmbalagemId { get; set; }

        public virtual Embalagem Embalagem { get; set; }

        public decimal? ComprimentoMinimo { get; set; }

        public decimal? ComprimentoMaximo { get; set; }

        public decimal? LarguraMinimo { get; set; }

        public decimal? LarguraMaximo { get; set; }

        public decimal? AlturaMinimo { get; set; }

        public decimal? AlturaMaximo { get; set; }

        public decimal? SomaDimensoesMinimo { get; set; }

        public decimal? SomaDimensoesMaximo { get; set; }

        public decimal? PesoMaximo { get; set; }

    }
}
