using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Services;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios
{
    public class CorreioShippingUpdateTask : ITask
    {
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly IAPICorreios _apiCorreios;

        public CorreioShippingUpdateTask(IOrderService orderService,
            ILogger logger,
            IAPICorreios apiCorreios)
        {
            _orderService = orderService;
            _logger = logger;
            _apiCorreios = apiCorreios;
        }

        public void Execute()
        {
            var lstShippingStatus = new List<int>
            {
                (int)ShippingStatus.Shipped,
                (int)ShippingStatus.PartiallyShipped
            };

            var ordersShipped = _orderService.SearchOrders(ssIds: lstShippingStatus);
            
            try
            {
                foreach (var order in ordersShipped)
                {
                    foreach (var shipment in order.Shipments)
                    {
                        _apiCorreios.CheckShipmentDelived(shipment);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Plugin.Shipping.Correios: Erro atualização status de rastreamento", ex);
            }            
        }
    }
}
