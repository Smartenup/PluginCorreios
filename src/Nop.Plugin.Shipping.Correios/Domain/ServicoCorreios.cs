using Nop.Core;


namespace Nop.Plugin.Shipping.Correios.Domain
{
    public class ServicoCorreios : BaseEntity
    {
        public string CodigoServico { get; set; }
        public string Descricao { get; set;}

        public string DescricaoShippingMethod { get; set;}
        public string CodigoSegmento { get; set; }
        public string DescricaoSegmento { get; set; }
        public bool GrandeFormato { get; set; }
        public virtual ValorDeclarado ValorDeclarado { get; set; }        
    }
}
