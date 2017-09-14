using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;


namespace Nop.Plugin.Shipping.Correios.Domain
{
	public class CorreiosServices
	{

        public const string PRIMEIRO_LISTA_MAIS_BARATO = "PRIMEIRO";

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
				//case "41106" : return "PAC sem contrato";
                case "4510": return "PAC sem contrato";
                case "04510": return "PAC sem contrato";

                //case "40010" : return "SEDEX sem contrato";
                case "4014": return "SEDEX sem contrato";
                case "04014": return "SEDEX sem contrato";

                case "40045" : return "SEDEX a Cobrar, sem contrato";
				case "40126": return "SEDEX a Cobrar, com contrato";
				case "40215": return "SEDEX 10, sem contrato";
				case "40290": return "SEDEX Hoje, sem contrato";
                case "40169": return "SEDEX 12, com contrato";

                //case "40096": return "SEDEX com contrato";
                case "04162": return "SEDEX com contrato";
                case "4162": return "SEDEX com contrato";

                case "40436": return "SEDEX com contrato (40436)";
				case "40444": return "SEDEX com contrato (40444)";
                case "04693": return "PAC Grandes Volumes";//41300
                case "81019": return "e-SEDEX, com contrato";

                
                

                //case "41068": return "PAC com contrato";
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
				//case "PAC sem contrato": return "41106";
                case "PAC sem contrato": return "04510";
                //case "SEDEX sem contrato": return "40010";
                case "SEDEX sem contrato": return "04014";

                case "SEDEX a Cobrar, sem contrato": return "40045";
				case "SEDEX a Cobrar, com contrato": return "40126";
				case "SEDEX 10, sem contrato": return "40215";
				case "SEDEX Hoje, sem contrato": return "40290";

                //case "SEDEX com contrato": return "40096";
                case "SEDEX com contrato": return "04162";

                case "SEDEX com contrato (40436)": return "40436";
				case "SEDEX com contrato (40444)": return "40444";
				case "e-SEDEX, com contrato": return "81019";

                //case "PAC com contrato": return "41068";
                case "PAC com contrato": return "04669"; 

                case "SEDEX com contrato (40568)": return "40568";
				case "SEDEX com contrato (40606)": return "40606";
				case "(Grupo 1) e-SEDEX, com contrato": return "81868";
				case "(Grupo 2) e-SEDEX, com contrato": return "81833";
				case "(Grupo 3) e-SEDEX, com contrato": return "81850";
                case "PAC Grandes Volumes": return "04693";
                case "SEDEX 12, com contrato": return "40169";


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
                
                case "40169": return "SEDEX 12";
				case "40215": return "SEDEX 10";
				case "40290": return "SEDEX Hoje";
                //case "40096":
                case "4162":
                case "04162":
                case "40436":
				case "40444":
                
                //case "40010":
                case "04014":
                case "4014":

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
                
                //case "41106":
                case "4510":
                case "04510":

                //case "41068":
                case "4669":
                case "04669":
                    {
						return "PAC";
					}
                case "04693": return "PAC Grandes Volumes";
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

        #endregion
    }


    public class InspectorBehavior : IEndpointBehavior
    {
        public string LastRequestXML
        {
            get
            {
                return myMessageInspector.LastRequestXML;
            }
        }

        public string LastResponseXML
        {
            get
            {
                return myMessageInspector.LastResponseXML;
            }
        }


        private MyMessageInspector myMessageInspector = new MyMessageInspector();
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {

        }

        public void Validate(ServiceEndpoint endpoint)
        {

        }


        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(myMessageInspector);
        }
    }


    public class MyMessageInspector : IClientMessageInspector
    {
        public string LastRequestXML { get; private set; }
        public string LastResponseXML { get; private set; }
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            LastResponseXML = reply.ToString();
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
        {
            LastRequestXML = request.ToString();
            return request;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public partial class Envelope
    {

        private EnvelopeHeader headerField;

        private EnvelopeBody bodyField;

        /// <remarks/>
        public EnvelopeHeader Header
        {
            get
            {
                return this.headerField;
            }
            set
            {
                this.headerField = value;
            }
        }

        /// <remarks/>
        public EnvelopeBody Body
        {
            get
            {
                return this.bodyField;
            }
            set
            {
                this.bodyField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public partial class EnvelopeHeader
    {

        private string xOPNETTransactionTraceField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("X-OPNET-Transaction-Trace", Namespace = "http://opnet.com")]
        public string XOPNETTransactionTrace
        {
            get
            {
                return this.xOPNETTransactionTraceField;
            }
            set
            {
                this.xOPNETTransactionTraceField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public partial class EnvelopeBody
    {

        private buscaEventosResponse buscaEventosResponseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://resource.webservice.correios.com.br/")]
        public buscaEventosResponse buscaEventosResponse
        {
            get
            {
                return this.buscaEventosResponseField;
            }
            set
            {
                this.buscaEventosResponseField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://resource.webservice.correios.com.br/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://resource.webservice.correios.com.br/", IsNullable = false)]
    public partial class buscaEventosResponse
    {

        private @return returnField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "")]
        public @return @return
        {
            get
            {
                return this.returnField;
            }
            set
            {
                this.returnField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class @return
    {

        private decimal versaoField;

        private byte qtdField;

        private returnObjeto objetoField;

        /// <remarks/>
        public decimal versao
        {
            get
            {
                return this.versaoField;
            }
            set
            {
                this.versaoField = value;
            }
        }

        /// <remarks/>
        public byte qtd
        {
            get
            {
                return this.qtdField;
            }
            set
            {
                this.qtdField = value;
            }
        }

        /// <remarks/>
        public returnObjeto objeto
        {
            get
            {
                return this.objetoField;
            }
            set
            {
                this.objetoField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class returnObjeto
    {

        private string numeroField;

        private string siglaField;

        private string nomeField;

        private string categoriaField;

        private returnObjetoEvento[] eventoField;

        /// <remarks/>
        public string numero
        {
            get
            {
                return this.numeroField;
            }
            set
            {
                this.numeroField = value;
            }
        }

        /// <remarks/>
        public string sigla
        {
            get
            {
                return this.siglaField;
            }
            set
            {
                this.siglaField = value;
            }
        }

        /// <remarks/>
        public string nome
        {
            get
            {
                return this.nomeField;
            }
            set
            {
                this.nomeField = value;
            }
        }

        /// <remarks/>
        public string categoria
        {
            get
            {
                return this.categoriaField;
            }
            set
            {
                this.categoriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("evento")]
        public returnObjetoEvento[] evento
        {
            get
            {
                return this.eventoField;
            }
            set
            {
                this.eventoField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class returnObjetoEvento
    {

        private string tipoField;

        private byte statusField;

        private string dataField;

        private string horaField;

        private string descricaoField;

        private string detalheField;

        private string localField;

        private uint codigoField;

        private string cidadeField;

        private string ufField;

        private returnObjetoEventoDestino destinoField;

        /// <remarks/>
        public string tipo
        {
            get
            {
                return this.tipoField;
            }
            set
            {
                this.tipoField = value;
            }
        }

        /// <remarks/>
        public byte status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public string data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }

        /// <remarks/>
        public string hora
        {
            get
            {
                return this.horaField;
            }
            set
            {
                this.horaField = value;
            }
        }

        /// <remarks/>
        public string descricao
        {
            get
            {
                return this.descricaoField;
            }
            set
            {
                this.descricaoField = value;
            }
        }

        /// <remarks/>
        public string detalhe
        {
            get
            {
                return this.detalheField;
            }
            set
            {
                this.detalheField = value;
            }
        }

        /// <remarks/>
        public string local
        {
            get
            {
                return this.localField;
            }
            set
            {
                this.localField = value;
            }
        }

        /// <remarks/>
        public uint codigo
        {
            get
            {
                return this.codigoField;
            }
            set
            {
                this.codigoField = value;
            }
        }

        /// <remarks/>
        public string cidade
        {
            get
            {
                return this.cidadeField;
            }
            set
            {
                this.cidadeField = value;
            }
        }

        /// <remarks/>
        public string uf
        {
            get
            {
                return this.ufField;
            }
            set
            {
                this.ufField = value;
            }
        }

        /// <remarks/>
        public returnObjetoEventoDestino destino
        {
            get
            {
                return this.destinoField;
            }
            set
            {
                this.destinoField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class returnObjetoEventoDestino
    {

        private string localField;

        private uint codigoField;

        private string cidadeField;

        private string bairroField;

        private string ufField;

        /// <remarks/>
        public string local
        {
            get
            {
                return this.localField;
            }
            set
            {
                this.localField = value;
            }
        }

        /// <remarks/>
        public uint codigo
        {
            get
            {
                return this.codigoField;
            }
            set
            {
                this.codigoField = value;
            }
        }

        /// <remarks/>
        public string cidade
        {
            get
            {
                return this.cidadeField;
            }
            set
            {
                this.cidadeField = value;
            }
        }

        /// <remarks/>
        public string bairro
        {
            get
            {
                return this.bairroField;
            }
            set
            {
                this.bairroField = value;
            }
        }

        /// <remarks/>
        public string uf
        {
            get
            {
                return this.ufField;
            }
            set
            {
                this.ufField = value;
            }
        }
    }
}
