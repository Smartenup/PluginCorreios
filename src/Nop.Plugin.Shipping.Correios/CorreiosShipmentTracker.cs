using Nop.Plugin.Shipping.Correios.Services;
using Nop.Services.Shipping.Tracking;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios
{
    public class CorreiosShipmentTracker : IShipmentTracker
	{
        private readonly IAPICorreios _apiCorreios;


        public CorreiosShipmentTracker(IAPICorreios apiCorreios)
        {
            _apiCorreios = apiCorreios;
        }

		public bool IsMatch(string trackingNumber)
		{
			if (string.IsNullOrWhiteSpace(trackingNumber))
				return false;

			return trackingNumber.Length == 13;
		}

		public string GetUrl(string trackingNumber)
		{
            return string.Empty;

		}

		public IList<ShipmentStatusEvent> GetShipmentEvents(string trackingNumber)
        {
            return _apiCorreios.GetShipmentEventsAsync(trackingNumber).GetAwaiter().GetResult();
        }

    }
}
