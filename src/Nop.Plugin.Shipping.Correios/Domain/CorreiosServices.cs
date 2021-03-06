﻿using System;


namespace Nop.Plugin.Shipping.Correios.Domain
{
	public class CorreiosServices
	{

        public const string PRIMEIRO_LISTA_MAIS_BARATO = "PRIMEIRO";
        public const decimal CONST_VALOR_DECLARADO_MINIMO_PAC = 50; 
        public const decimal CONST_VALOR_DECLARADO_MINIMO_SEDEX = 75;

        /// <summary>
        /// Correios Service names
        /// </summary>
        private string[] _services = {
										"PAC sem contrato",
										"SEDEX sem contrato",
										"SEDEX a Cobrar, sem contrato",
										"SEDEX a Cobrar, com contrato",
                                        "SEDEX 12",
                                        "SEDEX 10, sem contrato",
										"SEDEX Hoje, sem contrato",
										"SEDEX com contrato",
										"SEDEX com contrato (40436)",
										"SEDEX com contrato (40444)",
                                        "PAC Grandes Volumes",
                                        "e-SEDEX, com contrato",
										"PAC com contrato",
										"SEDEX com contrato (40568)",
										"SEDEX com contrato (40606)",
										"(Grupo 1) e-SEDEX, com contrato",
										"(Grupo 2) e-SEDEX, com contrato",
										"(Grupo 3) e-SEDEX, com contrato"
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
		public static string GetServiceName(string serviceId)
		{
			switch (serviceId)
			{

                case "4510": return "PAC sem contrato";
                case "04510": return "PAC sem contrato";
                case "4014": return "SEDEX sem contrato";
                case "04014": return "SEDEX sem contrato";
                case "40045" : return "SEDEX a Cobrar, sem contrato";
				case "40126": return "SEDEX a Cobrar, com contrato";
				case "40215": return "SEDEX 10";
				case "40290": return "SEDEX Hoje";
                case "40169": return "SEDEX 12";
                case "04162": return "SEDEX com contrato";
                case "4162": return "SEDEX com contrato";
                case "40436": return "SEDEX com contrato (40436)";
				case "40444": return "SEDEX com contrato (40444)";
                case "04693": return "PAC Grandes Volumes";
                case "81019": return "e-SEDEX, com contrato";               
                case "04669": return "PAC com contrato";
                case "4669": return "PAC com contrato";
                case "40568": return "SEDEX com contrato (40568)";
				case "40606": return "SEDEX com contrato (40606)";
				case "81868": return "(Grupo 1) e-SEDEX, com contrato";
				case "81833": return "(Grupo 2) e-SEDEX, com contrato";
				case "81850": return "(Grupo 3) e-SEDEX, com contrato";


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
                case "PAC sem contrato": return "04510";
                case "SEDEX sem contrato": return "04014";
                case "SEDEX a Cobrar, sem contrato": return "40045";
				case "SEDEX a Cobrar, com contrato": return "40126";
				case "SEDEX 10": return "40215";
				case "SEDEX Hoje": return "40290";
                case "SEDEX com contrato": return "04162";
                case "SEDEX com contrato (40436)": return "40436";
				case "SEDEX com contrato (40444)": return "40444";
				case "e-SEDEX, com contrato": return "81019";
                case "PAC com contrato": return "04669"; 
                case "SEDEX com contrato (40568)": return "40568";
				case "SEDEX com contrato (40606)": return "40606";
				case "(Grupo 1) e-SEDEX, com contrato": return "81868";
				case "(Grupo 2) e-SEDEX, com contrato": return "81833";
				case "(Grupo 3) e-SEDEX, com contrato": return "81850";
                case "PAC Grandes Volumes": return "04693";
                case "SEDEX 12": return "40169";

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


        public static Decimal ObterValorDeclaroMinimoServico(string codigoServico)
        {
            switch (codigoServico)
            {

                case "40169": 
                case "40215": 
                case "40290": 
                case "4162":
                case "04162":
                case "40436":
                case "40444":
                case "04014":
                case "4014":
                case "40568":
                case "40606": 
                case "40045":
                case "40126":
                case "81019":
                case "81868":
                case "81833":
                case "81850": { return CONST_VALOR_DECLARADO_MINIMO_SEDEX; }
                case "4510":
                case "04510":
                case "4669":
                case "04669": 
                case "04693": { return CONST_VALOR_DECLARADO_MINIMO_PAC; }
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
                
                case "40169": return "SEDEX 12";
				case "40215": return "SEDEX 10";
				case "40290": return "SEDEX Hoje";
                case "4162":
                case "04162":
                case "40436":
				case "40444":
                case "04014":
                case "4014":
                case "40568":
				case "40606": return "SEDEX";
				case "40045":
				case "40126": return "SEDEX a Cobrar";
                case "4510":
                case "04510":
                case "4669":
                case "04669": return "PAC";
                case "04693": return "PAC Grandes Volumes";
				case "81019":
				case "81868":
				case "81833":
				case "81850": return "e-SEDEX";

				default: return "Desconhecido";
			}
		}


		public static bool ValidateServicePublicName(string publicName)
        {
            switch (publicName)
            {

                case "SEDEX 12":
                case "SEDEX 10": 
                case "SEDEX Hoje":
                case "SEDEX":
                case "SEDEX a Cobrar":
                case "PAC": 
                case "PAC Grandes Volumes":
                case "e-SEDEX":
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
                string descricaoServico = GetServiceName(codigoServico);

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
