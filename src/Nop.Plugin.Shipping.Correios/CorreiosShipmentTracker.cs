using System.Collections.Generic;
using Nop.Services.Logging;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Shipping.Correios
{
    public class CorreiosShipmentTracker : IShipmentTracker
	{
		private readonly ILogger _logger;
		private readonly CorreiosSettings _correiosSettings;

		public CorreiosShipmentTracker(ILogger logger, CorreiosSettings correiosSettings)
		{
            _logger = logger;
            _correiosSettings = correiosSettings;
		}

		public bool IsMatch(string trackingNumber)
		{
			if (string.IsNullOrWhiteSpace(trackingNumber))
				return false;

			return trackingNumber.Length == 13;
		}

		public string GetUrl(string trackingNumber)
		{
			return "http://websro.correios.com.br/sro_bin/txect01$.QueryList?P_LINGUA=001&P_TIPO=001&P_COD_UNI=" + trackingNumber.ToUpperInvariant();
		}

		public IList<ShipmentStatusEvent> GetShipmentEvents(string trackingNumber)
		{
			//TODO
			return new List<ShipmentStatusEvent>();
		}
	}
}
