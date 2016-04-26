namespace Nop.Plugin.Shipping.Correios.Domain
{
	public class CorreiosServices
	{
		/// <summary>
		/// Correios Service names
		/// </summary>
		private string[] _services = {
										"PAC sem contrato",
										"SEDEX sem contrato",
										"SEDEX a Cobrar, sem contrato",
										"SEDEX a Cobrar, com contrato",
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
				case "41106" : return "PAC sem contrato";
				case "40010" : return "SEDEX sem contrato";
				case "40045" : return "SEDEX a Cobrar, sem contrato";
				case "40126": return "SEDEX a Cobrar, com contrato";
				case "40215": return "SEDEX 10, sem contrato";
				case "40290": return "SEDEX Hoje, sem contrato";
				case "40096": return "SEDEX com contrato";
				case "40436": return "SEDEX com contrato (40436)";
				case "40444": return "SEDEX com contrato (40444)";
                case "41300": return "PAC Grandes Volumes";
                case "81019": return "e-SEDEX, com contrato";
				case "41068": return "PAC com contrato";
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
				case "PAC sem contrato": return "41106";
				case "SEDEX sem contrato": return "40010";
				case "SEDEX a Cobrar, sem contrato": return "40045";
				case "SEDEX a Cobrar, com contrato": return "40126";
				case "SEDEX 10, sem contrato": return "40215";
				case "SEDEX Hoje, sem contrato": return "40290";
				case "SEDEX com contrato": return "40096";
				case "SEDEX com contrato (40436)": return "40436";
				case "SEDEX com contrato (40444)": return "40444";
				case "e-SEDEX, com contrato": return "81019";
				case "PAC com contrato": return "41068";
				case "SEDEX com contrato (40568)": return "40568";
				case "SEDEX com contrato (40606)": return "40606";
				case "(Grupo 1) e-SEDEX, com contrato": return "81868";
				case "(Grupo 2) e-SEDEX, com contrato": return "81833";
				case "(Grupo 3) e-SEDEX, com contrato": return "81850";
                case "PAC Grandes Volumes": return "41300";

                default: return "Desconhecido";
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
				case "40215": return "SEDEX 10";
				case "40290": return "SEDEX Hoje";
				case "40096":
				case "40436":
				case "40444":
				case "40010":
				case "40568":
				case "40606":
					{
						return "SEDEX";
					}
				case "40045":
				case "40126":
					{
						return "SEDEX a Cobrar";
					}

				case "41068":
				case "41106":
					{
						return "PAC";
					}
                case "41300": return "PAC Grandes Volumes";
				case "81019":
				case "81868":
				case "81833":
				case "81850":
					{
						return "e-SEDEX";
					}

				default: return "Desconhecido";
			}
		}
		#endregion
	}
}
