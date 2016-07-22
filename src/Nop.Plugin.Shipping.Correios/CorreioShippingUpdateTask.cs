using Newtonsoft.Json.Linq;
using Nop.Core.Domain.Orders;
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
            var ordersShipped = _orderService.SearchOrders(os: OrderStatus.Processing, ss: Core.Domain.Shipping.ShippingStatus.Shipped
            );

            var ordersPartiallyShipped = _orderService.SearchOrders(os: OrderStatus.Processing, ss: Core.Domain.Shipping.ShippingStatus.PartiallyShipped
            );


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

        private void CheckMarkShipmentDelivered(Core.Domain.Shipping.Shipment shipment)
        {

            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);

            EndpointAddress address = new EndpointAddress("http://webservice.correios.com.br:80/service/rastro");

            wsRastro.ServiceClient wsRastro = new wsRastro.ServiceClient(binding, address);

            try
            {
                string rastreio = wsRastro.RastroJson("ECT", "SRO", "L", "U", "101", shipment.TrackingNumber);

                if (string.IsNullOrWhiteSpace(rastreio) || rastreio.Equals("{}"))
                    return;

                JObject jObject = JObject.Parse(rastreio);

                string lastShipmentTipo = GetLastShipmentTipo(jObject);
                string lastShipmentStatus = GetLastShipmentStatus(jObject);

                if (lastShipmentTipo.Equals("BDE", StringComparison.InvariantCultureIgnoreCase) ||
                     lastShipmentTipo.Equals("BDI", StringComparison.InvariantCultureIgnoreCase) ||
                     lastShipmentTipo.Equals("BDR", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (lastShipmentStatus.Equals("01", StringComparison.InvariantCultureIgnoreCase) ||
                        lastShipmentStatus.Equals("02", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _orderProcessingService.Deliver(shipment, true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Plugin.Shipping.Correios: Erro na atualização de status de rastreamento, Ordem " + shipment.OrderId.ToString() + " - rastreio " + shipment.TrackingNumber, ex);
            }
            finally
            {
                if(wsRastro.State != CommunicationState.Closed)
                    wsRastro.Close();
            }
            
        }

        private static string GetLastShipmentStatus(JObject jObject)
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

        private static string GetLastShipmentTipo(JObject jObject)
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
