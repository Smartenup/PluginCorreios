using Newtonsoft.Json.Linq;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml.Serialization;

namespace Nop.Plugin.Shipping.Correios
{
    public class CorreioShippingUpdateTask : ITask
    {
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private wsRastro.ServiceClient _wsRastro;
        private InspectorBehavior _requestInterceptor;


        public CorreioShippingUpdateTask(IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _logger = logger;
        }

        public void Execute()
        {
            var lstShippingStatus = new List<int>();

            lstShippingStatus.Add((int)Core.Domain.Shipping.ShippingStatus.Shipped);
            lstShippingStatus.Add((int)Core.Domain.Shipping.ShippingStatus.PartiallyShipped);

            var ordersShipped = _orderService.SearchOrders(ssIds: lstShippingStatus);
            
            try
            {
                ConfigurarWsRastro();

                foreach (var order in ordersShipped)
                {
                    foreach (var shipment in order.Shipments)
                    {
                        CheckMarkShipmentDelivered(shipment);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Plugin.Shipping.Correios: Erro atualização status de rastreamento", ex);
            }
            finally
            {
                FecharWsRastro();
            }
        }


        public void ConfigurarWsRastro()
        {
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);

            binding.OpenTimeout = new TimeSpan(0, 10, 0);
            binding.CloseTimeout = new TimeSpan(0, 10, 0);
            binding.SendTimeout = new TimeSpan(0, 10, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);

            EndpointAddress address = new EndpointAddress("http://webservice.correios.com.br:80/service/rastro");

            _wsRastro = new wsRastro.ServiceClient(binding, address);

            _requestInterceptor = new InspectorBehavior();
            _wsRastro.Endpoint.Behaviors.Add(_requestInterceptor);
        }

        public void FecharWsRastro()
        {
            if (_wsRastro.State != CommunicationState.Closed)
                _wsRastro.Close();
        }


        private void CheckMarkShipmentDelivered(Core.Domain.Shipping.Shipment shipment)
        {
            if (shipment.DeliveryDateUtc.HasValue)
                return;
            try
            {
                _wsRastro.buscaEventos("ECT", "SRO", "L", "U", "101", shipment.TrackingNumber.ToUpperInvariant());

                var ser = new XmlSerializer(typeof(Envelope));
                var envelope = new Envelope();

                using (TextReader reader = new StringReader(_requestInterceptor.LastResponseXML))
                {
                    envelope = (Envelope)ser.Deserialize(reader);
                }

                returnObjeto objetoRastreado = envelope.Body.buscaEventosResponse.@return.objeto;

                foreach (var evento in objetoRastreado.evento)
                {
                    if (evento.tipo.Equals("BDE", StringComparison.InvariantCultureIgnoreCase) ||
                        evento.tipo.Equals("BDI", StringComparison.InvariantCultureIgnoreCase) ||
                        evento.tipo.Equals("BDR", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (evento.status == 1 || evento.status == 2)
                        {
                            string logDelivered = string.Format("Plugin.Shipping.Correios: Entregue {0} - Ordem {1}",
                                shipment.TrackingNumber, shipment.OrderId.ToString());

                            _logger.Information(logDelivered);

                            _orderProcessingService.Deliver(shipment, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string logError = string.Format("Plugin.Shipping.Correios: Erro atualização status de rastreamento, Ordem {0} - Rastreio {1}",
                    shipment.OrderId.ToString(),
                    shipment.TrackingNumber);

                _logger.Error(logError, ex, (Core.Domain.Customers.Customer)shipment.Order.Customer);
            }
            
        }
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
