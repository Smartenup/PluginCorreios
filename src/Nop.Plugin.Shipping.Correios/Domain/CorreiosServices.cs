using System;


namespace Nop.Plugin.Shipping.Correios.Domain
{
	public class CorreiosServices
	{

        public const string PRIMEIRO_LISTA_MAIS_BARATO = "PRIMEIRO";
        public const decimal CONST_VALOR_DECLARADO_MINIMO_PAC = 50; 
        public const decimal CONST_VALOR_DECLARADO_MINIMO_SEDEX = 75;
        public const string CONST_CODIGO_VALOR_DECLARADO_PAC = "064";
        public const string CONST_CODIGO_VALOR_DECLARADO_SEDEX = "019";

        /// <summary>
        /// Correios Service names
        /// </summary>
        private string[] _services = {
                                        "PAC sem contrato (04510)",
                                        "SEDEX sem contrato (04014)",
                                        "PAC sem contrato (04510)",
                                        "SEDEX sem contrato (04014)",
                                        "SEDEX HOJE CONTRATO (03204)",
                                        "SEDEX 10 CONTRATO (03158)",
                                        "SEDEX 12 CONTRATO (03140)",
                                        "SEDEX com contrato (03220)",
                                        "SEDEX CONTRATO GDES FORMATOS (03212)",
                                        "PAC CONTRATO GDES FORMATOS (03328)",
                                        "PAC CONTRATO (03298)",
                                        "PAC MINI CONTRATO (04227)"
                                        };

		#region Properties

		/// <summary>
		/// Correios services string names
		/// </summary>
		public string[] Services
		{
			get { return _services; }
		}

		#endregion

		#region Utilities
		/// <summary>
		/// Gets the text name based on the ServiceID (in Correios Reply)
		/// </summary>
		/// <param name="serviceId">ID of the carrier service -from Correios</param>
		/// <returns>String representation of the carrier service</returns>
		public static string GetServiceName(int serviceId)
		{

			switch (serviceId)
			{
                case 4510: return "PAC sem contrato (04510)";
                case 4014: return "SEDEX sem contrato (04014)";
				case 3204: return "SEDEX HOJE CONTRATO (03204)";
                case 3158: return "SEDEX 10 CONTRATO (03158)";
                case 3140: return "SEDEX 12 CONTRATO (03140)";
                case 3220: return "SEDEX com contrato (03220)";
                case 3212: return "SEDEX CONTRATO GDES FORMATOS (03212)";
                case 3328: return "PAC CONTRATO GDES FORMATOS (03328)";
                case 3298: return "PAC CONTRATO (03298)";
                case 4227: return "PAC MINI CONTRATO (04227)";

                default: return "Desconhecido";
			}
		}

		/// <summary>
		/// Gets the ServiceId based on the text name
		/// </summary>
		/// <param name="serviceName">Name of the carrier service (based on the text name returned from GetServiceName())</param>
		/// <returns>Service ID as used by Correios</returns>
		public static string GetServiceId(string serviceName)
		{
			switch (serviceName)
			{
                case "PAC sem contrato (04510)": return "04510";
                case "SEDEX sem contrato (04014)": return "04014";
                case "SEDEX HOJE CONTRATO (03204)": return "03204";
                case "SEDEX 10 CONTRATO (03158)": return "03158";
                case "SEDEX 12 CONTRATO (03140)": return "03140";
                case "SEDEX CONTRATO (03220)": return "03220";
                case "SEDEX CONTRATO GDES FORMATOS (03212)": return "03212";
                case "PAC CONTRATO GDES FORMATOS (03328)": return "03328";
                case "PAC CONTRATO (03298)": return "03298";
                case "PAC MINI CONTRATO (04227)": return "04227";

                case "SEDEX": return "03220";
                case "PAC": return "03298";

                default: return "Desconhecido";
			}
		}


        public static Decimal ObterValorDeclarado(decimal orderTotal, string codigoServico)
        {

            decimal retorno = 0;


            decimal valorMinimo = ObterValorDeclaroMinimoServico(codigoServico);

            if (orderTotal >= valorMinimo)
            {
                retorno = orderTotal;
            }
            else
            {
                retorno = valorMinimo;
            }

            return retorno;
                        
        }

        public static string ObterCodigoValorDeclarado(string codigoServico)
        {
            switch (codigoServico)
            {
                case "04014":
                case "03204":
                case "03158":
                case "03140":
                case "03220":
                case "03212": return CONST_CODIGO_VALOR_DECLARADO_SEDEX;
                case "04510":
                case "03328":
                case "03298":
                case "04227": { return CONST_CODIGO_VALOR_DECLARADO_PAC; }
                default: return CONST_CODIGO_VALOR_DECLARADO_SEDEX;
            }
        }

        public static Decimal ObterValorDeclaroMinimoServico(string codigoServico)
        {
            switch (codigoServico)
            {

                case "04014":
                case "03204":
                case "03158":
                case "03140":
                case "03220":
                case "03212": return CONST_VALOR_DECLARADO_MINIMO_SEDEX;
                case "04510":
                case "03328":
                case "03298":
                case "04227": { return CONST_VALOR_DECLARADO_MINIMO_PAC; }
                default: return CONST_VALOR_DECLARADO_MINIMO_PAC;

            }
        }
		/// <summary>
		/// Gets the public text name based on the ServiceID.
		/// </summary>
		/// <param name="serviceId">ID of the carrier service -from Correios.</param>
		/// <returns>String representation of the carrier service (public name)</returns>
		public static string GetServicePublicNameById(string serviceId)
		{
			switch (serviceId)
			{
                
                case "04510": return "PAC sem contrato (04510)";
                case "04014": return "SEDEX sem contrato (04014)";
				case "03204": return "SEDEX HOJE CONTRATO (03204)";
                case "03158": return "SEDEX 10 CONTRATO (03158)";
                case "03140": return "SEDEX 12 CONTRATO (03140)";
                case "03220": return "SEDEX com contrato (03220)";
                case "03212": return "SEDEX CONTRATO GDES FORMATOS (03212)";
                case "03328": return "PAC CONTRATO GDES FORMATOS (03328)";
                case "03298": return "PAC CONTRATO (03298)";
                case "04227": return "PAC MINI CONTRATO (04227)";


				default: return "Desconhecido";
			}
		}


		public static bool ValidateServicePublicName(string publicName)
        {
            switch (publicName)
            {

                case "PAC sem contrato (04510)":
                case "SEDEX sem contrato (04014)":
                case "SEDEX HOJE CONTRATO (03204)":
                case "SEDEX 10 CONTRATO (03158)":
                case "SEDEX 12 CONTRATO (03140)":
                case "SEDEX CONTRATO (03220)":
                case "SEDEX CONTRATO GDES FORMATOS (03212)":
                case "PAC CONTRATO GDES FORMATOS (03328)":
                case "PAC CONTRATO (03298)":
                case "PAC MINI CONTRATO (04227)":
                case "SEDEX":
                case "PAC":
                    {
                        return true;
                    }
                default: return false;
            }
        }


        public static string ObterCodigoEnvio(string shippingMethod, string servicosOferecidos)
        {
            string shippingMethodPedido = shippingMethod;

            string codigoServicosOferecidos = servicosOferecidos;

            foreach (var codigoServico in codigoServicosOferecidos.Split(','))
            {
                string descricaoServico = GetServiceName(int.Parse(codigoServico));

                string publicName = GetServicePublicNameById(codigoServico);

                if (shippingMethodPedido.Contains("[Frete Grátis]"))
                {
                    shippingMethodPedido = shippingMethodPedido.Replace("[Frete Grátis]", string.Empty).Trim();
                }

                if (descricaoServico.Contains(shippingMethodPedido) && publicName.Equals(shippingMethodPedido, StringComparison.InvariantCultureIgnoreCase))
                {
                    return codigoServico;
                }
            }

            throw new Exception("Codigo de servico não encontrado");
        }

        #endregion
    }

   




    
}
