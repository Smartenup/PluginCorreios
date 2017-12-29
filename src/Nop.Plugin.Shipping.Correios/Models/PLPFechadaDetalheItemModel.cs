using Nop.Web.Framework.Mvc;
using System.ComponentModel;

namespace Nop.Plugin.Shipping.Correios.Models
{
    public class PLPFechadaDetalheItemModel : BaseNopModel
    {
        [DisplayName("PLP ID Nop")]
        public int Id { get; set; }
        [DisplayName("Código de Rastreio")]
        public string TrackingNumber { get; set; }
        [DisplayName("Pedido")]
        public int OrderId { get; set; }
        [DisplayName("Nome Destinatário")]
        public string NomeDestinatario { get; set; }
        [DisplayName("CEP Destinatário")]
        public string CepDestinatario { get; set; }
        [DisplayName("Peso")]
        public decimal Peso { get; set; }
        [DisplayName("Peso Efetivado")]
        public decimal PesoEfetivo { get; set; }
        [DisplayName("Valor Declarado")]
        public decimal ValorDeclarado { get; set; }
        [DisplayName("Valor Frete")]
        public decimal ValorFrete { get; set; }
        [DisplayName("Valor Frete Efetivado")]
        public decimal ValorFreteEfetivado { get; set; }
    }
}
