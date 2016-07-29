using Newtonsoft.Json.Linq;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.ServiceModel;

namespace Nop.Plugin.Shipping.Correios
{
    public class CorreioShippingUpdateTask : ITask
    {
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private wsRastro.ServiceClient _wsRastro;


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
            var ordersShipped = _orderService.SearchOrders(ss: Core.Domain.Shipping.ShippingStatus.Shipped
            );

            var ordersPartiallyShipped = _orderService.SearchOrders(ss: Core.Domain.Shipping.ShippingStatus.PartiallyShipped
            );

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

                foreach (var order in ordersPartiallyShipped)
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


        private void ConfigurarWsRastro()
        {
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);

            binding.OpenTimeout = new TimeSpan(0, 10, 0);
            binding.CloseTimeout = new TimeSpan(0, 10, 0);
            binding.SendTimeout = new TimeSpan(0, 10, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);

            EndpointAddress address = new EndpointAddress("http://webservice.correios.com.br:80/service/rastro");

            _wsRastro = new wsRastro.ServiceClient(binding, address);
        }

        private void FecharWsRastro()
        {
            if (_wsRastro.State != CommunicationState.Closed)
                _wsRastro.Close();
        }


        private void CheckMarkShipmentDelivered(Core.Domain.Shipping.Shipment shipment)
        {
            if (shipment.DeliveryDateUtc.HasValue)
                return;

            string tracking = string.Empty;

            try
            {
                tracking = _wsRastro.RastroJson("ECT", "SRO", "L", "U", "101", shipment.TrackingNumber.ToUpperInvariant());

                if (string.IsNullOrWhiteSpace(tracking) || tracking.Equals("{}"))
                {
                    string logTrackingEmpty = string.Format("Plugin.Shipping.Correios: Rastreio {0} - Ordem {1} - RetornoCorreios ({2})",
                        shipment.TrackingNumber, shipment.OrderId.ToString(), tracking);

                    _logger.Information(logTrackingEmpty);

                    return;
                }

                JObject jObject = JObject.Parse(tracking);

                string lastShipmentTipo = GetLastShipmentTipo(jObject);
                string lastShipmentStatus = GetLastShipmentStatus(jObject);

                string logInformation = string.Format("Plugin.Shipping.Correios: Rastreio {0} - Status {1} - Tipo {2} - Ordem {3}",
                    shipment.TrackingNumber, lastShipmentStatus, lastShipmentTipo, shipment.OrderId.ToString());

                _logger.Information(logInformation);

                if (lastShipmentTipo.Equals("BDE", StringComparison.InvariantCultureIgnoreCase) ||
                     lastShipmentTipo.Equals("BDI", StringComparison.InvariantCultureIgnoreCase) ||
                     lastShipmentTipo.Equals("BDR", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (lastShipmentStatus.Equals("01", StringComparison.InvariantCultureIgnoreCase) ||
                        lastShipmentStatus.Equals("02", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string logDelivered = string.Format("Plugin.Shipping.Correios: Entregue {0} - Ordem {1}",
                            shipment.TrackingNumber, shipment.OrderId.ToString());

                        _logger.Information(logDelivered);

                        _orderProcessingService.Deliver(shipment, true);
                    }
                }
            }
            catch (Exception ex)
            {
                string logError = string.Format("Plugin.Shipping.Correios: Erro atualização status de rastreamento, Ordem {0} - Rastreio {1} - RetornoCorreios {2}",
                    shipment.OrderId.ToString(),
                    shipment.TrackingNumber,
                    tracking);

                _logger.Error(logError, ex, shipment.Order.Customer);
            }
            
        }

        private string GetLastShipmentStatus(JObject jObject)
        {
            try
            {

                try
                {
                    return (string)jObject["sroxml"]["objeto"]["evento"][0]["status"];
                }
                catch (Exception)
                {
                    return (string)jObject["sroxml"]["objeto"]["evento"]["status"];
                }

            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private string GetLastShipmentTipo(JObject jObject)
        {
            try
            {

                try
                {
                    return (string)jObject["sroxml"]["objeto"]["evento"][0]["tipo"];
                }
                catch (Exception)
                {
                    return (string)jObject["sroxml"]["objeto"]["evento"]["tipo"];
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }
    }
}
