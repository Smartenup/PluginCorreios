using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Plugin.Shipping.Correios.Domain.SigepWeb.Xml;
using Nop.Plugin.Shipping.Correios.Util;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public class SigepWebPlpService : ISigepWebPlpService
    {
        private const long MAX_RECEIVED_MESSAGE_SIZE = 2147483647;
        private const int MAX_BUFFER_SIZE = 2147483647;

        private readonly CorreiosSettings _correiosSettings;
        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;
        private readonly ISigepWebService _sigepWebService;
        private readonly IWorkContext _workContext;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly ShippingSettings _shippingSettings;
        private readonly IAddressService _addressService;
        private readonly IPdfSigepWebService _pdfSigepWebService;
        private readonly IOrderProcessingService _orderProcessingService;




        public SigepWebPlpService(CorreiosSettings correiosSettings,
            IOrderService orderService,
            IShipmentService shipmentService,
            ISigepWebService sigepWebService,
            IWorkContext workContext,
            IAddressAttributeParser addressAttributeParser,
            ShippingSettings shippingSettings,
            IAddressService addressService,
            IPdfSigepWebService pdfSigepWebService,
            IOrderProcessingService orderProcessingService
            )
        {
            _correiosSettings = correiosSettings;
            _orderService = orderService;
            _shipmentService = shipmentService;
            _sigepWebService = sigepWebService;
            _workContext = workContext;
            _addressAttributeParser = addressAttributeParser;
            _shippingSettings = shippingSettings;
            _addressService = addressService;
            _pdfSigepWebService = pdfSigepWebService;
            _orderProcessingService = orderProcessingService;
        }

        public PlpSigepWeb FecharPlp()
        {
            PlpSigepWeb plpSigebWeb = ObterPlpEmAberto();

            var principal = new Principal();

            var xmlPlp = principal.ObterXmlPrincipal(plpSigebWeb, _correiosSettings, _shippingSettings, _addressService, _addressAttributeParser, _workContext, _shipmentService);

            List<string> lstTrackingNumber = ObterListaEnvioSemCodigoVerificador(plpSigebWeb.PlpSigepWebShipments);

            long? idPlpCorreios = null;

            string xmlString = string.Concat(xmlPlp.Declaration.ToString(), xmlPlp.ToString(System.Xml.Linq.SaveOptions.DisableFormatting));

            FecharPlpCorreios(xmlString, plpSigebWeb.Id, lstTrackingNumber.ToArray(), out idPlpCorreios);

            plpSigebWeb.XmlPLP = xmlString;

            plpSigebWeb.PlpSigepWebCorreiosId = idPlpCorreios;

            plpSigebWeb.PlpStatusId = (int)PlpSigepWebStatus.Fechada;

            plpSigebWeb.FechamentoOnUtc = DateTime.UtcNow;

            _sigepWebService.UpdatePlp(plpSigebWeb);

            ///Marcar os itens como enviados
            IList<Shipment> lstShipment = ObterPedidos(plpSigebWeb);

            foreach (var shipment in lstShipment)
            {
                _orderProcessingService.Ship(shipment, true);
            }

            return plpSigebWeb;
        }

        public PlpSigepWeb ObterPlp(int Id)
        {
            PlpSigepWeb plpSigebWeb = _sigepWebService.GetPlp(Id);

            return plpSigebWeb;
        }



        public PlpSigepWeb ObterPlpEmAberto()
        {
            PlpSigepWeb plpSigebWeb = _sigepWebService.GetPlp(PlpSigepWebStatus.Aberta);

            if (plpSigebWeb == null)
            {
                plpSigebWeb = ObterNovaPLP();
                _sigepWebService.InsertPlp(plpSigebWeb);
            }

            return plpSigebWeb;
        }




        public wsAtendeClienteService.clienteERP ObterClienteSigepWEB()
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);

            binding.MaxReceivedMessageSize = MAX_RECEIVED_MESSAGE_SIZE;
            binding.MaxBufferSize = MAX_BUFFER_SIZE;

            var address = new EndpointAddress(ObterEndPointAtendeCliente());

            wsAtendeClienteService.AtendeClienteClient ws = new wsAtendeClienteService.AtendeClienteClient(binding, address);

            wsAtendeClienteService.clienteERP clienteSigepWEB = null;

            try
            {
                clienteSigepWEB = ws.buscaCliente(_correiosSettings.NumeroContratoSIGEP, _correiosSettings.CartaoPostagemSIGEP, _correiosSettings.UsuarioSIGEP, _correiosSettings.SenhaSIGEP);
            }
            finally
            {
                ws.Close();
            }

            return clienteSigepWEB;
        }

        public void VerificarStatusCartaoPostegem()
        {

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);

            var address = new EndpointAddress(ObterEndPointAtendeCliente());

            wsAtendeClienteService.AtendeClienteClient ws = new wsAtendeClienteService.AtendeClienteClient(binding, address);

            try
            {
                wsAtendeClienteService.statusCartao status = ws.getStatusCartaoPostagem(_correiosSettings.CartaoPostagemSIGEP, _correiosSettings.UsuarioSIGEP, _correiosSettings.SenhaSIGEP);

                if (status != wsAtendeClienteService.statusCartao.Normal)
                    throw new Exception("Cartão não esta em status normal, postagem no SIGEPWEB não esta habilitada");

            }
            finally
            {
                ws.Close();
            }

        }

        public void FecharPlpCorreios(string xmlPlp, long idPlpNop, string[] listaEtiquetas, out long? numeroPlpCorreios)
        {

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);

            var address = new EndpointAddress(ObterEndPointAtendeCliente());

            wsAtendeClienteService.AtendeClienteClient ws = new wsAtendeClienteService.AtendeClienteClient(binding, address);

            try
            {
                numeroPlpCorreios = ws.fechaPlpVariosServicos(xmlPlp, idPlpNop, _correiosSettings.CartaoPostagemSIGEP, listaEtiquetas, _correiosSettings.UsuarioSIGEP, _correiosSettings.SenhaSIGEP);
            }
            finally
            {
                ws.Close();
            }
        }

        public string ObterEndPointAtendeCliente()
        {
            if (_correiosSettings.AmbienteHomologacao)
                return "https://apphom.correios.com.br/SigepMasterJPA/AtendeClienteService/AtendeCliente?wsdl";
            else
                return "https://apps.correios.com.br/SigepMasterJPA/AtendeClienteService/AtendeCliente?wsdl";

        }


        public PlpSigepWebEtiqueta ObterProximaEtiquetaDisponivel(List<PlpSigepWebEtiqueta> lstEtiquetas, string shippingMethod)
        {
            string codigoServicoPedido = CorreiosServices.ObterCodigoEnvio(shippingMethod, _correiosSettings.CarrierServicesOffered);

            foreach (PlpSigepWebEtiqueta etiqueta in lstEtiquetas)
            {
                if ((etiqueta.CodigoServico.Equals(codigoServicoPedido, StringComparison.InvariantCultureIgnoreCase)) && (etiqueta.CodigoUtilizado == false))
                {
                    etiqueta.CodigoUtilizado = true;

                    _sigepWebService.UpdateEtiqueta(etiqueta);

                    return etiqueta;
                }
            }

            //Caso não tenha etiqueta retorna nulo
            return null;
        }

        public Shipment CriarShipmentNop(Order order, string trackingNumber)
        {
            ///Verificar se existe um Shipment ainda não enviado e excluir

            var listExcluir = new List<int>();

            foreach (var envio in order.Shipments)
            {
                if (string.IsNullOrWhiteSpace(envio.TrackingNumber) || envio.ShippedDateUtc is null)
                {
                    listExcluir.Add(envio.Id);
                }
            }

            if (listExcluir.Count > 0)
            {
                foreach (var item in listExcluir)
                {
                    Shipment shipmentDirty = _shipmentService.GetShipmentById(item);
                    _shipmentService.DeleteShipment(shipmentDirty);
                }
            }

            Shipment shipment = null;

            ///Cria o shipment para o pedido
            var orderItems = order.OrderItems;

            decimal? totalWeight = null;
            foreach (var orderItem in orderItems)
            {
                //is shippable
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                //ensure that this product can be shipped (have at least one item to ship)
                var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                if (maxQtyToAdd <= 0)
                    continue;

                int qtyToAdd = orderItem.Quantity; //parse quantity

                //validate quantity
                if (qtyToAdd <= 0)
                    continue;

                if (qtyToAdd > maxQtyToAdd)
                    qtyToAdd = maxQtyToAdd;

                int warehouseId = orderItem.Product.WarehouseId;

                var orderItemTotalWeight = orderItem.ItemWeight.HasValue ? orderItem.ItemWeight * qtyToAdd : null;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }
                if (shipment == null)
                {
                    shipment = new Shipment
                    {
                        OrderId = order.Id,
                        TrackingNumber = trackingNumber,
                        TotalWeight = null,
                        ShippedDateUtc = null,
                        DeliveryDateUtc = null,
                        AdminComment = string.Empty,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                }
                else
                {
                    shipment.TrackingNumber = trackingNumber;
                }
                //create a shipment item
                var shipmentItem = new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd,
                    WarehouseId = warehouseId
                };
                shipment.ShipmentItems.Add(shipmentItem);
            }

            //if we have at least one item in the shipment, then save it
            if (shipment != null && shipment.ShipmentItems.Any())
            {
                shipment.TotalWeight = totalWeight;
                _shipmentService.InsertShipment(shipment);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "A shipment has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }

            return shipment;
        }


        public List<PlpSigepWebEtiqueta> SolicitaEtiquetas(IList<Order> lstPedidos, wsAtendeClienteService.clienteERP cliente)
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferSize = 2147483647;

            var address = new EndpointAddress(ObterEndPointAtendeCliente());

            wsAtendeClienteService.AtendeClienteClient ws = new wsAtendeClienteService.AtendeClienteClient(binding, address);

            Dictionary<string, int> ltsCodigoQuantidade = ObterServicoQuantidade(lstPedidos);

            var lstServicoFaixaEtiqueta = new Dictionary<string, string>();

            var lstEtiquetas = new List<PlpSigepWebEtiqueta>();

            try
            {

                foreach (var codigoQuantidade in ltsCodigoQuantidade)
                {

                    foreach (wsAtendeClienteService.contratoERP contrato in cliente.contratos)
                    {

                        foreach (wsAtendeClienteService.cartaoPostagemERP cartaoPostagem in contrato.cartoesPostagem)
                        {

                            foreach (wsAtendeClienteService.servicoERP servico in cartaoPostagem.servicos)
                            {

                                if (servico.codigo.Trim().Equals(codigoQuantidade.Key, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string faixaEtiquetas = ws.solicitaEtiquetas("C", _correiosSettings.NumeroCNPJ, servico.id, codigoQuantidade.Value,
                                                            _correiosSettings.UsuarioSIGEP, _correiosSettings.SenhaSIGEP);


                                    string digitoInicioEtiqueta = faixaEtiquetas.Substring(0, 2);

                                    string digitoFinalEtiqueta = faixaEtiquetas.Substring(faixaEtiquetas.LastIndexOf(",") - 2, 2);

                                    string primeiraEtiqueta = faixaEtiquetas.Substring(2, faixaEtiquetas.LastIndexOf(",") - 4);

                                    for (int i = 0; i < codigoQuantidade.Value; i++)
                                    {
                                        var etiqueta = new PlpSigepWebEtiqueta();

                                        etiqueta.CodigoEtiquetaSemVerificador = digitoInicioEtiqueta +
                                            (int.Parse(primeiraEtiqueta) + i).ToString() + " " +
                                            digitoFinalEtiqueta;

                                        etiqueta.CodigoEtiquetaComVerificador = digitoInicioEtiqueta +
                                            CorreiosHelper.CalculaNumeroEtiquetaComVerificador((int.Parse(primeiraEtiqueta) + i).ToString()) +
                                            digitoFinalEtiqueta;

                                        etiqueta.CodigoServico = servico.codigo.Trim();

                                        etiqueta.CodigoUtilizado = false;

                                        _sigepWebService.InsertEtiqueta(etiqueta);

                                        lstEtiquetas.Add(etiqueta);
                                    }

                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                ws.Close();
            }


            return lstEtiquetas;
        }



        private IList<Shipment> ObterPedidos(PlpSigepWeb plpSigepWeb)
        {
            var lstShipment = new List<Shipment>();


            foreach (var item in plpSigepWeb.PlpSigepWebShipments)
            {
                Shipment shipment = _shipmentService.GetShipmentById(item.ShipmentId);

                lstShipment.Add(shipment);
            }

            return lstShipment;
        }
        private PlpSigepWeb ObterNovaPLP()
        {
            var plp = new PlpSigepWeb();

            plp.CreatedOnUtc = DateTime.UtcNow;
            plp.CustomerId = _workContext.CurrentCustomer.Id;
            plp.Deleted = false;
            plp.PlpStatusId = (int)PlpSigepWebStatus.Aberta;
            plp.XmlPLP = string.Empty;

            return plp;
        }


        private Dictionary<string, int> ObterServicoQuantidade(IList<Order> lstPedidos)
        {
            var list = new Dictionary<string, int>();

            var results = from o in lstPedidos
                          group o.ShippingMethod
                          by o.ShippingMethod
                  into g
                          select new { ShippingMethod = g.Key, Envios = g.ToList() };


            foreach (var item in results)
            {
                string codigoEnvio = CorreiosServices.ObterCodigoEnvio(item.ShippingMethod, _correiosSettings.CarrierServicesOffered);

                if (list.ContainsKey(codigoEnvio))
                    list[codigoEnvio] += item.Envios.Count();
                else
                    list.Add(codigoEnvio, item.Envios.Count());
            }

            return list;
        }


        private List<string> ObterListaEnvioSemCodigoVerificador(ICollection<PlpSigepWebShipment> plpSigepWebShipments)
        {
            var listaEnvio = new List<string>();

            foreach (var item in plpSigepWebShipments)
            {
                listaEnvio.Add(item.Etiqueta.CodigoEtiquetaSemVerificador.Replace(" ", string.Empty));
            }

            return listaEnvio;
        }




        private string ObterServicosAdicionais()
        {
            /*
             * Serviço Adicional Descrição
             * 01 - Aviso de Recebimento
             * 02 - Mão Própria Nacional
             * 19 - Valor Declarado Nacional (Encomendas)
             * 25 - Registro Nacional
             * 37 - Aviso de Recebimento Digital
             * 49 - Devolução de Nota Fiscal - SEDEX
             * 57 - Taxa de Entrega de Encomenda Despadronizada
             * 67 - Logística Reversa Simultânea Domiciliária
             * 69 - Logística Reversa Simultânea em Agência
             */

            string retorno = string.Empty;

            //O valor de registro nacional sempre deve ser enviado
            retorno = "25";

            string codigoAvisoRecebimento = "01";
            string codigoMaoPropria = "02";
            string codigoValorDeclarado = "19";

            if (_correiosSettings.IncluirAvisoRecebimento)
                retorno += codigoAvisoRecebimento;

            if (_correiosSettings.IncluirMaoPropria)
                retorno += codigoMaoPropria;

            if (_correiosSettings.IncluirValorDeclarado)
                retorno += codigoValorDeclarado;

            while (retorno.Length != 12)
                retorno += "0";

            return retorno;
        }


        public PlpSigepWebEtiqueta ObterProximaEtiquetaDisponivel(string nomeServicoPublico)
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferSize = 2147483647;

            var address = new EndpointAddress(ObterEndPointAtendeCliente());

            wsAtendeClienteService.AtendeClienteClient ws = new wsAtendeClienteService.AtendeClienteClient(binding, address);


            string codigoEnvio = CorreiosServices.ObterCodigoEnvio(nomeServicoPublico, _correiosSettings.CarrierServicesOffered);

            wsAtendeClienteService.clienteERP clienteERP = ObterClienteSigepWEB();


            try
            {
                foreach (wsAtendeClienteService.contratoERP contrato in clienteERP.contratos)
                {
                    foreach (wsAtendeClienteService.cartaoPostagemERP cartaoPostagem in contrato.cartoesPostagem)
                    {
                        foreach (wsAtendeClienteService.servicoERP servico in cartaoPostagem.servicos)
                        {

                            if (servico.codigo.Trim().Equals(codigoEnvio, StringComparison.InvariantCultureIgnoreCase))
                            {
                                string faixaEtiquetas = ws.solicitaEtiquetas("C", _correiosSettings.NumeroCNPJ, servico.id, 1,
                                                        _correiosSettings.UsuarioSIGEP, _correiosSettings.SenhaSIGEP);


                                string digitoInicioEtiqueta = faixaEtiquetas.Substring(0, 2);

                                string digitoFinalEtiqueta = faixaEtiquetas.Substring(faixaEtiquetas.LastIndexOf(",") - 2, 2);

                                string primeiraEtiqueta = faixaEtiquetas.Substring(2, faixaEtiquetas.LastIndexOf(",") - 4);

                                var etiqueta = new PlpSigepWebEtiqueta();

                                etiqueta.CodigoEtiquetaSemVerificador = digitoInicioEtiqueta +
                                    (int.Parse(primeiraEtiqueta) + 0).ToString() + " " +
                                    digitoFinalEtiqueta;

                                etiqueta.CodigoEtiquetaComVerificador = digitoInicioEtiqueta +
                                    CorreiosHelper.CalculaNumeroEtiquetaComVerificador((int.Parse(primeiraEtiqueta) + 0).ToString()) +
                                    digitoFinalEtiqueta;

                                etiqueta.CodigoServico = servico.codigo.Trim();

                                etiqueta.CodigoUtilizado = true;

                                _sigepWebService.InsertEtiqueta(etiqueta);

                                return etiqueta;
                            }
                        }
                    }
                }

            }
            finally
            {
                ws.Close();
            }

            return null;

        }
    }
}
