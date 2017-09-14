using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.Correios
{
    public class CorreiosSettings: ISettings
	{
		public string Url { get; set; }

		public string CodigoEmpresa { get; set; }

		public string Senha { get; set; }

		public decimal CustoAdicionalEnvio { get; set; }

		public bool IncluirMaoPropria { get; set; }

		public bool IncluirValorDeclarado { get; set; }

		public bool IncluirAvisoRecebimento { get; set; }

		public int DiasUteisAdicionais { get; set; }

		public string CarrierServicesOffered { get; set; }
        
        public bool FreteGratis { get; set; }
        
        public string CEPInicial { get; set; }
        
        public string CEPFinal { get; set; }
        
        public bool UtilizaValorMinimo { get; set; }
        
        public decimal ValorMinimo { get; set; }
        
        public string ServicoFreteGratis { get; set; }

        public bool MostrarTempoFabricacao { get; set; }

        public string UsuarioServicoRastreamento { get; set; }

        public string SenhaServicoRastreamento { get; set; }

        public string FreteGratisExcetoCustomerRoleIds { get; set; }
    }
}
