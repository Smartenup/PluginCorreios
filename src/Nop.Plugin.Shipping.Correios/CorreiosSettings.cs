using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.Correios
{
    public class CorreiosSettings: ISettings
	{
        #region Calculo Frete

        public string Url { get; set; }

		public string CodigoEmpresa { get; set; }

		public string Senha { get; set; }

		public decimal CustoAdicionalEnvio { get; set; }

		public bool IncluirMaoPropria { get; set; }

		public bool IncluirValorDeclarado { get; set; }

		public bool IncluirAvisoRecebimento { get; set; }

		public int DiasUteisAdicionais { get; set; }

		public string CarrierServicesOffered { get; set; }

        public bool MostrarTempoFabricacao { get; set; }

        #endregion
        
        #region FreteGratis

        public bool FreteGratis { get; set; }
        
        public string CEPInicial { get; set; }
        
        public string CEPFinal { get; set; }
        
        public bool UtilizaValorMinimo { get; set; }
        
        public decimal ValorMinimo { get; set; }
        
        public string ServicoFreteGratis { get; set; }

        public string FreteGratisExcetoCustomerRoleIds { get; set; }

        #endregion

        #region Serviço Rastreamento

        public string UsuarioServicoRastreamento { get; set; }

        public string SenhaServicoRastreamento { get; set; }


        #endregion  

        #region SIGEPWEB

        public bool AmbienteHomologacao { get; set; }

        public string CartaoPostagemSIGEP { get; set; }

        public string NumeroContratoSIGEP { get; set; }

        public string CodigoAdministrativoSIGEP { get; set; }

        public string UsuarioSIGEP { get; set; }

        public string SenhaSIGEP { get; set; }

        public string LogradouroRemetenteSIGEP { get; set; }

        public string NumeroRemetenteSIGEP { get; set; }

        public string ComplementoRemetenteSIGEP { get; set;}

        public string BairroRemetenteSIGEP { get; set; }

        public string EmailRemetenteSIGEP { get; set; }

        public bool UsarPesoPadraoSIGEP { get; set; }

        public string PesoPadraoSIGEP { get; set; } 

        public bool UsarDimensoesMinimasSIGEP { get; set; }

        public string NumeroDiretoria { get; set; }

        public string NumeroCNPJ { get; set; }

        public string NomeRemetenteSIGEP { get; set; }

        public string TelefoneRemetenteSIGEP { get; set; }

        public bool UtilizaValidacaoCEPEtiquetaSIGEP { get; set; }

        public bool ValidacaoServicoDisponivelCEPEtiquetaSIGEP { get; set; }

        #endregion
    }
}
