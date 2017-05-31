using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace Nop.Plugin.Shipping.Correios.Models
{
	public class CorreiosShippingModel
	{
		public CorreiosShippingModel()
		{
			CarrierServicesOffered = new List<string>();
			AvailableCarrierServices = new List<string>();
            AvailableCarrierServicesList = new List<SelectListItem>();
        }

		[DisplayName("URL")]
		public string Url { get; set; }

		[DisplayName("Código da Empresa")]
		public string CodigoEmpresa { get; set; }

		[DisplayName("Senha")]
		public string Senha { get; set; }

		[DisplayName("Custo adicional de envio")]
		public string CustoAdicionalEnvio { get; set; }

		[DisplayName("Mão Própria?")]
		public bool IncluirMaoPropria { get; set; }

		[DisplayName("Valor Declarado?")]
		public bool IncluirValorDeclarado { get; set; }

		[DisplayName("Aviso de Recebimento?")]
		public bool IncluirAvisoRecebimento { get; set; }

		[DisplayName("Dias uteis adicionais ao prazo")]
		public int DiasUteisAdicionais { get; set; }

		public IList<string> CarrierServicesOffered { get; set; }
		public IList<string> AvailableCarrierServices { get; set; }
        public IList<SelectListItem> AvailableCarrierServicesList { get; set; }

        public string[] CheckedCarrierServices { get; set; }

        [DisplayName("Utiliza Frete Grátis?")]
        public bool FreteGratis { get; set; }

        [DisplayName("Cep Inicial")]
        public string CEPInicial { get; set; }

        [DisplayName("Cep Final")]
        public string CEPFinal { get; set; }

        [DisplayName("Utiliza Valor Minimo")]
        public bool UtilizaValorMinimo { get; set; }

        [DisplayName("Valor Minimo")]
        public decimal ValorMinimo { get; set; }

        [DisplayName("Serviço para Frete Grátis")]
        public string ServicoFreteGratis { get; set; }

        [DisplayName("Mostrar Tempo Fabricação")]
        public bool MostrarTempoFabricacao { get; set; }

    }
}
