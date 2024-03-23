using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Nop.Plugin.Shipping.Correios.Models
{
    public class CorreiosShippingModel : BaseNopModel
    {
		public CorreiosShippingModel()
		{
			CarrierServicesOffered = new List<string>();
			AvailableCarrierServices = new List<string>();
            AvailableCarrierServicesList = new List<SelectListItem>();
            AvailableCustomerRoles = new List<SelectListItem>();
            SelectedCustomerRoleIds = new List<int>();
        }

        #region Calculo Frete

        [DisplayName("URL")]
		public string Url { get; set; }

		[DisplayName("Código da empresa")]
		public string CodigoEmpresa { get; set; }

		[DisplayName("Senha")]
		public string Senha { get; set; }

		[DisplayName("Custo adicional de envio")]
		public string CustoAdicionalEnvio { get; set; }

		[DisplayName("Mão própria?")]
		public bool IncluirMaoPropria { get; set; }

		[DisplayName("Valor declarado?")]
		public bool IncluirValorDeclarado { get; set; }

		[DisplayName("Aviso de recebimento?")]
		public bool IncluirAvisoRecebimento { get; set; }

		[DisplayName("Dias uteis adicionais ao prazo")]
		public int DiasUteisAdicionais { get; set; }

		public IList<string> CarrierServicesOffered { get; set; }
		public IList<string> AvailableCarrierServices { get; set; }
        public IList<SelectListItem> AvailableCarrierServicesList { get; set; }

        public string[] CheckedCarrierServices { get; set; }

        [DisplayName("Mostrar tempo fabricação")]
        public bool MostrarTempoFabricacao { get; set; }


        #endregion

        #region Frete Gratis

        [DisplayName("Utiliza frete grátis?")]
        public bool FreteGratis { get; set; }

        [DisplayName("Cep inicial")]
        public string CEPInicial { get; set; }

        [DisplayName("Cep final")]
        public string CEPFinal { get; set; }

        [DisplayName("Utiliza valor minimo")]
        public bool UtilizaValorMinimo { get; set; }

        [DisplayName("Valor minimo")]
        public decimal ValorMinimo { get; set; }

        [DisplayName("Serviço para frete grátis")]
        public string ServicoFreteGratis { get; set; }

        //customer roles
        [DisplayName("Frete gratis não disponivel para tais funções de cliente")]
        public string CustomerRoleNames { get; set; }
        public List<SelectListItem> AvailableCustomerRoles { get; set; }
        [DisplayName("Frete gratis exceto funções de cliente ")]
        [UIHint("MultiSelect")]
        public IList<int> SelectedCustomerRoleIds { get; set; }



        #endregion

        #region Serviço Rastreamento


        [DisplayName("Usuario serviço rastreamento")]
        public string UsuarioServicoRastreamento { get; set; }

        [DisplayName("Senha serviço rastreamento")]
        public string SenhaServicoRastreamento { get; set; }

        #endregion

        #region  SIGEPWEB

        [DisplayName("Cartão postagem")]
        public string CartaoPostagemSIGEP { get; set; }

        [DisplayName("Número contrato")]
        public string NumeroContratoSIGEP { get; set; }

        [DisplayName("Código administrativo")]
        public string CodigoAdministrativoSIGEP { get; set; }

        [DisplayName("Usuário")]
        public string UsuarioSIGEP { get; set; }

        [DisplayName("Senha")]
        public string SenhaSIGEP { get; set; }

        [DisplayName("Diretoria")]
        public string Diretoria { get; set; }

        [DisplayName("Logradouro remetente")]
        public string LogradouroRemetenteSIGEP { get; set; }

        [DisplayName("Número remetente")]
        public string NumeroRemetenteSIGEP { get; set; }

        [DisplayName("Complemento remetente")]
        public string ComplementoRemetenteSIGEP { get; set; }

        [DisplayName("Bairro remetente")]
        public string BairroRemetenteSIGEP { get; set; }

        [DisplayName("E-mail remetente")]
        public string EmailRemetenteSIGEP { get; set; }

        [DisplayName("Usar peso padrão")]
        public bool UsarPesoPadraoSIGEP { get; set; }

        [DisplayName("Peso padrão PLP (gramas)")]
        public string PesoPadraoSIGEP { get; set; }

        [DisplayName("Usar dimensões minimas")]
        public bool UsarDimensoesMinimasSIGEP { get; set; }

        [DisplayName("Número do CNPJ")]
        public string NumeroCNPJ { get; set; }

        [DisplayName("Ambiente de Homologação")]
        public bool AmbienteHomologacao { get; set; }

        [DisplayName("Nome Remetente")]
        public string NomeRemetenteSIGEP { get; set; }

        [DisplayName("Telefone Remetente")]
        public string TelefoneRemetenteSIGEP { get; set; }

        [DisplayName("Utiliza validação de CEP ao gerar etiqueta")]
        public bool UtilizaValidacaoCEPEtiquetaSIGEP { get; set; }

        [DisplayName("Utiliza validação de serviço disponível ao CEP ao gerar etiqueta")]
        public bool ValidacaoServicoDisponivelCEPEtiquetaSIGEP { get; set; }


        [DisplayName("Chave API Correios gerada no portal correios")]
        public string ChaveAPICorreios { get; set; }

        #endregion

    }
}
