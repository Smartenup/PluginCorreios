using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Rastro;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public class APICorreios : IAPICorreios
    {

        private Domain.CorreiosAPI.Token.Token _token;
        private Domain.CorreiosAPI.Token.Token TokenAPI 
        {
            get
            {
                if (
                    (_token == null) ||
                    (_token.ExpiraEm <= DateTime.Now)
                   )
                {
                    _token = GetTokenAPIAsync();
                }
                return _token;
            }          
        }

        private readonly ILogger _logger;
        private readonly CorreiosSettings _correiosSettings;
        private readonly IOrderService _orderService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IShipmentService _shipmentService;
        public APICorreios(ILogger logger, 
            CorreiosSettings correiosSettings,
            IOrderService orderService,
            IWorkflowMessageService workflowMessageService,
            IWorkContext workContext,
            IOrderProcessingService orderProcessingService,
            IShipmentService shipmentService)
        {
            _logger = logger;
            _correiosSettings = correiosSettings;
            _orderService = orderService;
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
            _orderProcessingService = orderProcessingService;
            _shipmentService = shipmentService;
        }
        
        public async Task<IList<ShipmentStatusEvent>> GetShipmentEventsAsync(string trackingNumber)
        {
            var lstShipmentEvents = new List<ShipmentStatusEvent>();

            var rastro = await  GetRastroAsync(trackingNumber);

            foreach (var objeto in rastro.Objetos)
            {
                if (objeto.Eventos == null)
                {
                    continue;
                }

                foreach (var evento in objeto.Eventos)
                {
                    var shipmentStatusEvent = new ShipmentStatusEvent();

                    shipmentStatusEvent.EventName = GetEventName(evento);
                    shipmentStatusEvent.Date = evento.DtHrCriado;
                    shipmentStatusEvent.Location = evento.Unidade?.Endereco?.Uf + " - " + evento.Unidade?.Endereco?.Cidade;
                    shipmentStatusEvent.CountryCode = "BR";

                    lstShipmentEvents.Add(shipmentStatusEvent);
                }
            }

            return lstShipmentEvents;
            
        }


        private string GetEventName(Evento evento)
        {
            var sb = new StringBuilder();

            sb.AppendLine(evento.Descricao);

            if (!string.IsNullOrWhiteSpace(evento.Detalhe))
                sb.AppendLine(" - " + evento.Detalhe);

            if (!string.IsNullOrWhiteSpace( evento.Unidade?.Endereco?.Cidade)  )
            {
                if (!string.IsNullOrEmpty(evento.Unidade?.Tipo))
                {
                    sb.AppendLine("- de " + evento.Unidade?.Tipo + ", " + evento.Unidade?.Endereco?.Cidade + " - " + evento.Unidade?.Endereco?.Uf);
                }
            }

            if (!string.IsNullOrWhiteSpace(evento.UnidadeDestino?.Endereco?.Cidade))
            {
                if (!string.IsNullOrEmpty(evento.UnidadeDestino?.Tipo))
                {
                    sb.AppendLine("- para " + evento.UnidadeDestino?.Tipo + ", " + evento.UnidadeDestino?.Endereco?.Cidade + " - " + evento.UnidadeDestino?.Endereco?.Uf);
                }
            }
            return sb.ToString();

        }


        public async Task<Rastro> GetRastroAsync(string trackingNumber)
        {
            Rastro rastro = null;

            string url = string.Concat("https://api.correios.com.br/srorastro/v1/objetos/", trackingNumber, "?resultado=T");

            var myUri = new Uri(url);
            var myWebRequest = WebRequest.Create(myUri);
            var myHttpWebRequest = (HttpWebRequest)myWebRequest;
            myHttpWebRequest.PreAuthenticate = true;
            myHttpWebRequest.Headers.Add("Authorization", "Bearer " + TokenAPI.token);
            myHttpWebRequest.Accept = "application/json";

            var myWebResponse = myWebRequest.GetResponse();
            var responseStream = myWebResponse.GetResponseStream();
            if (responseStream == null) return null;

            var myStreamReader = new StreamReader(responseStream, Encoding.UTF8);
            var json = myStreamReader.ReadToEnd();

            responseStream.Close();
            myWebResponse.Close();

            if (!string.IsNullOrWhiteSpace(json))
            {
                rastro = JsonConvert.DeserializeObject<Rastro>(json);
            }

            return rastro;

        }


        public Domain.CorreiosAPI.Token.Token GetTokenAPIAsync()
        {
            Domain.CorreiosAPI.Token.Token token = null;

            var url = "https://api.correios.com.br/token/v1/autentica/contrato";

            
            var authenticationString = $"{_correiosSettings.NumeroCNPJ}:{_correiosSettings.ChaveAPICorreios}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(authenticationString));


            var json = JsonConvert.SerializeObject(new { numero = _correiosSettings.NumeroContratoSIGEP, });
            var encodedJson = Encoding.ASCII.GetBytes(json);

            var request = (HttpWebRequest)WebRequest.Create(url);

            request.ContentType = "application/json";
            request.Method = "POST";
            request.ContentLength = encodedJson.Length;
            request.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);
            request.PreAuthenticate = true;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(encodedJson, 0, encodedJson.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (!string.IsNullOrWhiteSpace(responseString))
                token = JsonConvert.DeserializeObject<Domain.CorreiosAPI.Token.Token>(HttpUtility.HtmlDecode(responseString));

            response.Close();

            return token;
        }

        public void CheckShipmentDelived(Shipment shipment)
        {
            if (shipment.DeliveryDateUtc.HasValue)
                return;

            try
            {
                var rastro = GetRastroAsync(shipment.TrackingNumber).GetAwaiter().GetResult();

                foreach (var objeto in rastro.Objetos)
                {
                    if (objeto.Eventos == null)
                    {
                        continue;
                    }

                    foreach (var evento in objeto.Eventos)
                    {
                        CheckDelivered(shipment, evento);

                        CheckWaitingWithdrawal(shipment, evento);

                        CheckStolen(shipment, evento);

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

        private void CheckStolen(Shipment shipment, Evento evento)
        {
            if (evento.Codigo.Equals("BDE", StringComparison.InvariantCultureIgnoreCase) ||
                evento.Codigo.Equals("BDI", StringComparison.InvariantCultureIgnoreCase) ||
                evento.Codigo.Equals("BDR", StringComparison.InvariantCultureIgnoreCase))
            {
                if (evento.Tipo == "50" || evento.Tipo == "51" || evento.Tipo == "52")
                {

                    var notaLinhaDigitavel = shipment.Order.OrderNotes.Where(note => note.Note.Contains(evento.Descricao));

                    ///If doesn't exists description of Stolen
                    if (notaLinhaDigitavel.Count() == 0)
                    {
                        AddOrderNote(ObterDescricaoObjetoRoubado(evento), true, shipment.Order, true);

                        string logObjetoRoubado = string.Format("Plugin.Shipping.Correios: {0} {1} - Ordem {2}",
                        evento.Descricao, shipment.TrackingNumber, shipment.OrderId);

                        _logger.Information(logObjetoRoubado);
                    }
                }
            }
        }


       

        private void CheckWaitingWithdrawal(Shipment shipment, Evento evento)
        {
            if (evento.Codigo.Equals("LDI", StringComparison.InvariantCultureIgnoreCase))
            {
                if (evento.Tipo == "00" || evento.Tipo == "01" || evento.Tipo == "02" || evento.Tipo =="03"  || evento.Tipo == "14")
                {
                    var noteDescription = shipment.Order.OrderNotes.Where(note => note.Note.Contains(evento.Descricao));

                    ///If doesn't exists description of withdraw
                    if (noteDescription.Count() == 0)
                    {
                        AddOrderNote(ObterDescricaoDisponivelRetirada(evento), true, shipment.Order, true);

                        string logWaitingWithdrawal = string.Format("Plugin.Shipping.Correios: {0} {1} - Ordem {2}",
                        evento.Descricao, shipment.TrackingNumber, shipment.OrderId);

                        _logger.Information(logWaitingWithdrawal);
                    }
                }
            }
        }

        private void CheckDelivered(Shipment shipment, Evento evento)
        {
            if (evento.Codigo.Equals("BDE", StringComparison.InvariantCultureIgnoreCase) ||
                evento.Codigo.Equals("BDI", StringComparison.InvariantCultureIgnoreCase) ||
                evento.Codigo.Equals("BDR", StringComparison.InvariantCultureIgnoreCase))
            {
                if (evento.Tipo == "00" || evento.Tipo == "01")
                {

                    _orderProcessingService.Deliver(shipment, true);

                    string logDelivered = string.Format("Plugin.Shipping.Correios: Entregue {0} - Ordem {1}",
                        shipment.TrackingNumber, shipment.OrderId.ToString());

                    _logger.Information(logDelivered);

                    var shipmentDate = _shipmentService.GetShipmentById(shipment.Id);

                    IFormatProvider culture = new CultureInfo("pt-BR", true);
                    var brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                    DateTime dataAtualizacaoCorreios = evento.DtHrCriado;


                    shipmentDate.DeliveryDateUtc = TimeZoneInfo.ConvertTimeToUtc(dataAtualizacaoCorreios, brasiliaTimeZone);

                    _shipmentService.UpdateShipment(shipmentDate);
                }

            }
        }


        private void AddOrderNote(string note, bool showNoteToCustomer, Order order, bool sendEmail = false)
        {
            var orderNote = new Nop.Core.Domain.Orders.OrderNote();
            orderNote.CreatedOnUtc = DateTime.UtcNow;
            orderNote.DisplayToCustomer = showNoteToCustomer;
            orderNote.Note = note;
            
            order.OrderNotes.Add(orderNote);


            _orderService.UpdateOrder(order);

            //new order notification
            if (sendEmail)
            {
                orderNote.Order = order;
                //email
                _workflowMessageService.SendNewOrderNoteAddedCustomerNotification(
                    orderNote, _workContext.WorkingLanguage.Id);
            }
        }

        private string ObterDescricaoDisponivelRetirada(Evento evento)
        {
            var str = new StringBuilder();

            str.AppendLine(evento.Descricao + "-");
            str.AppendLine(evento.Detalhe);
            str.AppendLine("Unidade - " + evento.Unidade.Tipo);
            str.AppendLine(evento.Unidade.Endereco?.Logradouro + " " + evento.Unidade.Endereco?.Numero);
            str.AppendLine(evento.Unidade.Endereco?.Complemento);
            str.AppendLine(evento.Unidade.Endereco?.Bairro);
            str.AppendLine(evento.Unidade.Endereco?.Cidade + " / " + evento.Unidade.Endereco?.Uf);
            str.AppendLine("Data Limite Retirada: " + evento.DtLimiteRetirada?.ToString());

            return str.ToString();
        }

        private string ObterDescricaoObjetoRoubado(Evento evento)
        {
            var str = new StringBuilder();

            str.AppendLine(evento.Descricao + "-");
            str.AppendLine(evento.Detalhe);
            str.AppendLine("Unidade - " + evento.Unidade.Tipo);
            str.AppendLine(evento.Unidade.Endereco?.Logradouro + " " + evento.Unidade.Endereco?.Numero);
            str.AppendLine(evento.Unidade.Endereco?.Complemento);
            str.AppendLine(evento.Unidade.Endereco?.Bairro);
            str.AppendLine(evento.Unidade.Endereco?.Cidade + " / " + evento.Unidade.Endereco?.Uf);

            return str.ToString();
        }
    }
}
