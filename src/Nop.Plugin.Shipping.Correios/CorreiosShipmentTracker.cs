using Nop.Plugin.Shipping.Correios.Domain.Serialization;
using Nop.Services.Logging;
using Nop.Services.Shipping.Tracking;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Xml.Serialization;

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
            ///return "http://websro.correios.com.br/sro_bin/txect01$.QueryList?P_LINGUA=001&P_TIPO=001&P_COD_UNI=" + trackingNumber.ToUpperInvariant();
            return string.Empty;

		}

		public IList<ShipmentStatusEvent> GetShipmentEvents(string trackingNumber)
		{
            //TODO
            var lstShipmentEvents = new List<ShipmentStatusEvent>();
            wsRastro.ServiceClient _wsRastro = null;

            try
            {
                var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);

                binding.OpenTimeout = new TimeSpan(0, 10, 0);
                binding.CloseTimeout = new TimeSpan(0, 10, 0);
                binding.SendTimeout = new TimeSpan(0, 10, 0);
                binding.ReceiveTimeout = new TimeSpan(0, 10, 0);

                var address = new EndpointAddress("http://webservice.correios.com.br:80/service/rastro");

                _wsRastro = new wsRastro.ServiceClient(binding, address);

                var _requestInterceptor = new InspectorBehavior();
                _wsRastro.Endpoint.Behaviors.Add(_requestInterceptor);

                if (string.IsNullOrWhiteSpace(_correiosSettings.UsuarioServicoRastreamento))
                    _wsRastro.buscaEventos("ECT", "SRO", "L", "T", "101", trackingNumber.ToUpperInvariant());
                else
                    _wsRastro.buscaEventos(_correiosSettings.UsuarioServicoRastreamento,
                        _correiosSettings.SenhaServicoRastreamento, "L", "T", "101", trackingNumber.ToUpperInvariant());

                var ser = new XmlSerializer(typeof(Envelope));
                var envelope = new Envelope();

                using (TextReader reader = new StringReader(_requestInterceptor.LastResponseXML))
                {
                    envelope = (Envelope)ser.Deserialize(reader);
                }

                returnObjeto objetoRastreado = envelope.Body.buscaEventosResponse.@return.objeto;

                IFormatProvider culture = new CultureInfo("pt-BR", true);

                foreach (var evento in objetoRastreado.evento)
                {
                    var statusEvent = new ShipmentStatusEvent();

                    statusEvent.EventName = evento.local + " - " + evento.descricao;
                    statusEvent.Date = DateTime.ParseExact(evento.data + " " + evento.hora, "dd/MM/yyyy HH:mm", culture);
                    statusEvent.Location = evento.uf + " - " + evento.cidade;
                    statusEvent.CountryCode = "BR";
                    lstShipmentEvents.Add(statusEvent);
                }

            }
            catch(Exception ex)
            {
                _logger.Error("Plugin.Shipping.Correios: - Erro ao chamar WS dos Correios.\n" + ex.ToString(), ex);
            }
            finally
            {

                if (_wsRastro.State != CommunicationState.Closed)
                    _wsRastro.Close();
            }

            return lstShipmentEvents;


        }



	}
}
