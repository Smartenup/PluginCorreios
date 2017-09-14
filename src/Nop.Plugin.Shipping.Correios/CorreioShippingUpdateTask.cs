using Newtonsoft.Json.Linq;
using Nop.Plugin.Shipping.Correios.Domain;
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
        private CorreiosSettings _correiosSettings;


        public CorreioShippingUpdateTask(IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            CorreiosSettings correiosSettings)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _logger = logger;
            _correiosSettings = correiosSettings;
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
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);

            binding.OpenTimeout = new TimeSpan(0, 10, 0);
            binding.CloseTimeout = new TimeSpan(0, 10, 0);
            binding.SendTimeout = new TimeSpan(0, 10, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);

            var address = new EndpointAddress("http://webservice.correios.com.br:80/service/rastro");

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
                if(string.IsNullOrWhiteSpace(_correiosSettings.UsuarioServicoRastreamento))
                    _wsRastro.buscaEventos("ECT", "SRO", "L", "U", "101", shipment.TrackingNumber.ToUpperInvariant());
                else
                    _wsRastro.buscaEventos(_correiosSettings.UsuarioServicoRastreamento,
                        _correiosSettings.SenhaServicoRastreamento, "L", "U", "101", shipment.TrackingNumber.ToUpperInvariant());


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

                _logger.Error(logError, ex, shipment.Order.Customer);
            }
            
        }
    }

    
}
