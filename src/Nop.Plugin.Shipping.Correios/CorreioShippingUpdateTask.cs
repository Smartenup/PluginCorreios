using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Domain.Serialization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
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
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;


        public CorreioShippingUpdateTask(IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            CorreiosSettings correiosSettings,
            IWorkflowMessageService workflowMessageService,
            IWorkContext workContext)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _logger = logger;
            _correiosSettings = correiosSettings;
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
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


        private void CheckMarkShipmentDelivered(Shipment shipment)
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
                    VerificarPedidoEntregue(shipment, evento);

                    VerificarPedidoAguardandoRetirada(shipment, evento);
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

        private void VerificarPedidoAguardandoRetirada(Shipment shipment, returnObjetoEvento evento)
        {
            if (evento.tipo.Equals("LDI", StringComparison.InvariantCultureIgnoreCase))
            {
                if (evento.status == 0 || evento.status == 1 || evento.status == 2 || evento.status == 3 || evento.status == 14)
                {

                    var notaLinhaDigitavel = shipment.Order.OrderNotes.Where(note => note.Note.Contains(evento.descricao));

                    ///Caso não tenha anotação da descrição de retirada no local
                    if (notaLinhaDigitavel.Count() == 0)
                    {
                        AddOrderNote(ObterDescricaoDisponivelRetirada(evento), true, shipment.Order, true);

                        string logAguardandoRetirada = string.Format("Plugin.Shipping.Correios: {0} {1} - Ordem {2}",
                        evento.descricao, shipment.TrackingNumber, shipment.OrderId );

                        _logger.Information(logAguardandoRetirada);
                    }
                }
            }
        }

        private void VerificarPedidoEntregue(Shipment shipment, returnObjetoEvento evento)
        {
            if (evento.tipo.Equals("BDE", StringComparison.InvariantCultureIgnoreCase) ||
                evento.tipo.Equals("BDI", StringComparison.InvariantCultureIgnoreCase) ||
                evento.tipo.Equals("BDR", StringComparison.InvariantCultureIgnoreCase))
            {
                if (evento.status == 0 || evento.status == 1)
                {
                    _orderProcessingService.Deliver(shipment, true);

                    string logDelivered = string.Format("Plugin.Shipping.Correios: Entregue {0} - Ordem {1}",
                        shipment.TrackingNumber, shipment.OrderId.ToString());

                    _logger.Information(logDelivered);

                    
                }
            }
        }

        //Adiciona anotaçoes ao pedido
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
                //email
                _workflowMessageService.SendNewOrderNoteAddedCustomerNotification(
                    orderNote, _workContext.WorkingLanguage.Id);
            }
        }


        private string ObterDescricaoDisponivelRetirada(returnObjetoEvento evento)
        {
            var str = new StringBuilder();

            str.AppendLine(evento.descricao);
            str.AppendLine(evento.detalhe);
            str.AppendLine(evento.local);
            str.AppendLine(evento.endereco.logradouro + " " + evento.endereco.numero);
            str.AppendLine(evento.endereco.bairro);
            str.AppendLine(evento.endereco.localidade + " / " + evento.endereco.uf);

            return str.ToString();
        }

    }



}
