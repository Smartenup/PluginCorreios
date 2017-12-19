using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.Correios.Models
{
    public class EtiquetaModel : BaseNopModel
    {
        public string ImageTipo { get; set; }

        public string NomeEmpresaChancela { get; set; }
        public string TextoTipo { get; set; }

        public string DataMatrix { get; set; }

        public string Logo { get; set; }

        public string NFe { get; set; }

        public string OrderId { get; set; }

        public string OrderWeigth { get; set; }

        public string TrackingNumber { get; set; }

        public string BarcodeTN { get; set; }

        public string ToName { get; set; }

        public string ToAddress { get; set; }

        public string ToNumero { get; set; }

        public string ToComp { get; set; }

        public string ToBairro { get; set; }

        public string ToZIP { get; set; }

        public string ToCity { get; set; }

        public string ToUF { get; set; }

        public string BarcodeZIP { get; set; }

        public string Remetente { get; set; }

        public string EnderecoCompleto { get; set; }

        public string Bairro { get; set; }

        public string CEP { get; set; }

        public string Cidade { get; set; }

        public string UF { get; set; }
    }
}
