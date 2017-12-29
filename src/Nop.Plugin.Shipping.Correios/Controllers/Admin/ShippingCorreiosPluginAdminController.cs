using Nop.Admin.Controllers;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Plugin.Shipping.Correios.Models;
using Nop.Plugin.Shipping.Correios.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;



namespace Nop.Plugin.Shipping.Correios.Controllers.Admin
{
    public class ShippingCorreiosPluginAdminController : BaseAdminController
    {


        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;
        private readonly ISigepWebService _sigepWebService;
        private readonly IPdfSigepWebService _pdfSigepWebService;
        private readonly ISigepWebPlpService _sigepWebPlpService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerService _customerService;


        public ShippingCorreiosPluginAdminController(
            IOrderService orderService,
            IShipmentService shipmentService,
            ISigepWebService sigepWebService,
            IPdfSigepWebService pdfSigepWebService,
            ISigepWebPlpService sigepWebPlpService,
            IDateTimeHelper dateTimeHelper,
            ICustomerService customerService)
        {
            _orderService = orderService;
            _shipmentService = shipmentService;
            _sigepWebService = sigepWebService;
            _pdfSigepWebService = pdfSigepWebService;
            _sigepWebPlpService = sigepWebPlpService;
            _dateTimeHelper = dateTimeHelper;
            _customerService = customerService;
        }


        public ActionResult PLPFechadaDetalhe(int id)
        {
            var model = new PLPFechadaDetalheModel();

            var plp = _sigepWebService.GetPlp(id);

            Customer customer = _customerService.GetCustomerById(plp.CustomerId);

            model.Id = plp.Id;
            model.CorreiosId = plp.PlpSigepWebCorreiosId.Value;
            model.DataFechamento = plp.FechamentoOnUtc.Value;
            model.QuantidadeObjetos = plp.PlpSigepWebShipments.Count;
            model.UsuarioFechamento = customer.GetFullName();

            return View("~/Plugins/Shipping.Correios/Views/ShippingCorreios/PLPFechadaDetalhe.cshtml", model);

        }

        
        public ActionResult PLPFechadaDetalheSelect(int id, DataSourceRequest command)
        {
            PlpSigepWeb plpFechada = _sigepWebService.GetPlp(id);

            if (plpFechada == null)
            {
                return new JsonResult();
            }

            //Apresenta o resultado na tela
            var gridModel = new DataSourceResult
            {
                Total = plpFechada.PlpSigepWebShipments.Count()
            };

            var lstPlpFechadaDetalheItemModel = new List<PLPFechadaDetalheItemModel>();

            foreach (var item in plpFechada.PlpSigepWebShipments)
            {
                var detalheItemModel = new PLPFechadaDetalheItemModel();

                Shipment shipment = _shipmentService.GetShipmentById(item.ShipmentId);

                detalheItemModel.Id = item.Id;                
                detalheItemModel.ValorDeclarado = item.ValorDeclarado;
                detalheItemModel.OrderId = item.OrderId;
                detalheItemModel.TrackingNumber = item.Etiqueta.CodigoEtiquetaComVerificador;

                if (shipment !=  null)
                {
                    
                    detalheItemModel.NomeDestinatario = string.Format("{0} {1}", shipment.Order.ShippingAddress.FirstName, shipment.Order.ShippingAddress.LastName);
                    detalheItemModel.CepDestinatario = shipment.Order.ShippingAddress.ZipPostalCode;
                }

                detalheItemModel.Peso = item.PesoEstimado;
                detalheItemModel.PesoEfetivo = item.PesoEfetivado;
                detalheItemModel.ValorFrete = item.ValorEnvioEstimado;
                detalheItemModel.ValorFreteEfetivado = item.ValorEnvioEfetivado;

                lstPlpFechadaDetalheItemModel.Add(detalheItemModel);
            }

            gridModel.Data = lstPlpFechadaDetalheItemModel;


            return new JsonResult
            {
                Data = gridModel
            };
        }


        public ActionResult PLPFechada()
        {
            var model = new PLPFechadaSearchModel();

            return View("~/Plugins/Shipping.Correios/Views/ShippingCorreios/PLPFechada.cshtml", model);

        }

        public ActionResult PLPFechadaList(DataSourceRequest command, PLPFechadaSearchModel model)
        {

            DateTime? dataInicioValue = (model.DataInicio == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DataInicio.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? dataFinalValue = (model.DataFinal == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DataFinal.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var plps = _sigepWebService.ProcurarPlp(PlpSigepWebStatus.Fechada, dataInicioValue, dataFinalValue, 
                model.NumeroPedido, pageIndex: command.Page - 1, pageSize: command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = plps.Select(x =>
                {
                    Customer customer = _customerService.GetCustomerById(x.CustomerId);

                    return new PLPFechadaResultModel
                    {
                        Id = x.Id,
                        CorreiosId = x.PlpSigepWebCorreiosId.Value,
                        DataFechamento = _dateTimeHelper.ConvertToUserTime(x.FechamentoOnUtc.Value, DateTimeKind.Utc),
                        QuantidadeObjetos = x.PlpSigepWebShipments.Count(),
                        UsuarioFechamento = customer.GetFullName()
                    };
                }),
                Total = plps.TotalCount
            };

            return new JsonResult
            {
                Data = gridModel
            };

        }

        public ActionResult PdfFechamento(int id)
        {
            PlpSigepWeb plpSigepWeb = _sigepWebPlpService.ObterPlp(id);

            ///Imprimir o resumo da PLP
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfSigepWebService.PrintFechamentoToPdf(stream, plpSigepWeb);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, string.Format("plp{0}.pdf", plpSigepWeb.PlpSigepWebCorreiosId));

        }
        


        public ActionResult PLPAberta()
        {
            var model = new PLPAbertaSearchModel();

            return View("~/Plugins/Shipping.Correios/Views/ShippingCorreios/PLPAberta.cshtml", model);
        }

        
        public ActionResult PLPAbertaItemDelete(int id)
        {

            var shipmentPlp = _sigepWebService.GetPlpShipment(id);

            if (shipmentPlp == null)
                throw new ArgumentException("Nenhum item de plp encontrado com o id especificado");

            ///Liberar a etiqueta
            shipmentPlp.Etiqueta.CodigoUtilizado = false;

            _sigepWebService.UpdateEtiqueta(shipmentPlp.Etiqueta);

            ///Deletar o envio plp
            _sigepWebService.DeletePlpShipment(shipmentPlp);

            ///Excluir o shipment da order
            Shipment shipment = _shipmentService.GetShipmentById(shipmentPlp.ShipmentId);
            
            _shipmentService.DeleteShipment(shipment);

            return new NullJsonResult();
        }



        [HttpPost, ActionName("PLPAberta")]
        [FormValueRequired("pdf-etiqueta-all")]
        public ActionResult PdfEtiquetaAll(PLPAbertaSearchModel model)
        {
            PlpSigepWeb plpSigebWeb = _sigepWebPlpService.ObterPlpEmAberto();

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfSigepWebService.PrintEtiquetasToPdf(stream, plpSigebWeb.PlpSigepWebShipments.ToList());
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, string.Format("etiquetas{0}.pdf", DateTime.Now.ToFileTime()));
        }

        [HttpPost, ActionName("PLPFechadaDetalhe")]
        [FormValueRequired("pdf-etiqueta-all")]
        public ActionResult PdfEtiquetaFechadaAll(PLPFechadaDetalheModel model)
        {
            PlpSigepWeb plpSigebWeb = _sigepWebPlpService.ObterPlp(model.Id);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfSigepWebService.PrintEtiquetasToPdf(stream, plpSigebWeb.PlpSigepWebShipments.ToList());
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, string.Format("etiquetas{0}.pdf", DateTime.Now.ToFileTime()));
        }


        [HttpPost]
        public ActionResult PdfEtiquetaSelected(string selectedIds)
        {
            var sigepWebShipments = new List<PlpSigepWebShipment>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                sigepWebShipments.AddRange(_sigepWebService.GetShipmentsByIds(ids));
            }


            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfSigepWebService.PrintEtiquetasToPdf(stream, sigepWebShipments.ToList());
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, string.Format("etiquetas{0}.pdf", DateTime.Now.ToFileTime()));
        }




        public ActionResult PLPAbertaListSelect(DataSourceRequest command, PLPAbertaSearchModel model)
        {
            //Verificar o status do cartão de postagem no SIGEPWEB
            _sigepWebPlpService.VerificarStatusCartaoPostegem();

            //Verificar o cliente no SIGEPWEB
            wsAtendeClienteService.clienteERP cliente = _sigepWebPlpService.ObterClienteSigepWEB();

            //Cria ou atualiza uma PLP em Aberto no Nop
            PlpSigepWeb plpSigebWeb = _sigepWebPlpService.ObterPlpEmAberto();

            IList<Order> lstPedidos = ObterListaPedidos(model);
            var lstPedidosProblema = new List<Order>();

            if (lstPedidos.Count > 0)
            {
                //Solicita as etiquetas
                List<PlpSigepWebEtiqueta> lstEtiquetas = _sigepWebPlpService.SolicitaEtiquetas(lstPedidos, cliente);

                foreach (var pedido in lstPedidos)
                {
                    PlpSigepWebEtiqueta etiqueta = _sigepWebPlpService.ObterProximaEtiquetaDisponivel(lstEtiquetas, pedido.ShippingMethod);

                    if (etiqueta == null)
                    {
                        lstPedidosProblema.Add(pedido);
                        continue;
                    }

                    //Cria ou atualiza os Shipments do nop
                    Shipment shipment = _sigepWebPlpService.CriarShipmentNop(pedido, etiqueta.CodigoEtiquetaComVerificador);

                    //Cria ou atualiza a lista de envios da plp
                    var plpSigepWebShipment = new PlpSigepWebShipment();

                    plpSigepWebShipment.Deleted = false;
                    plpSigepWebShipment.EtiquetaId = etiqueta.Id;
                    plpSigepWebShipment.PlpSigepWebId = plpSigebWeb.Id;
                    plpSigepWebShipment.ShipmentId = shipment.Id;
                    plpSigepWebShipment.OrderId = shipment.OrderId;
                    plpSigepWebShipment.PesoEstimado = shipment.TotalWeight.HasValue ? shipment.TotalWeight.Value : 0;
                    plpSigepWebShipment.ValorEnvioEstimado = pedido.OrderShippingInclTax;
                    plpSigepWebShipment.ValorDeclarado = CorreiosServices.ObterValorDeclarado(pedido.OrderSubtotalInclTax, etiqueta.CodigoServico) ;

                    plpSigebWeb.PlpSigepWebShipments.Add(plpSigepWebShipment);
                }

                _sigepWebService.UpdatePlp(plpSigebWeb);

                model.OrdersIs = string.Empty;
            }


            plpSigebWeb = _sigepWebPlpService.ObterPlpEmAberto();

            //Apresenta o resultado na tela
            var gridModel = new DataSourceResult
            {
                Total = plpSigebWeb.PlpSigepWebShipments.Count()
            };

            var lstPlpAbertaResultModel = new List<PLPAbertaResultModel>();

            foreach (var item in plpSigebWeb.PlpSigepWebShipments)
            {
                var resulModel = new PLPAbertaResultModel();

                Shipment shipment = _shipmentService.GetShipmentById(item.ShipmentId);

                resulModel.Id = item.Id;
                resulModel.TrackingNumber = shipment.TrackingNumber;
                resulModel.OrderId = shipment.OrderId;
                resulModel.ValorDeclarado = item.ValorDeclarado;
                resulModel.NomeDestinatario = string.Format("{0} {1}", shipment.Order.ShippingAddress.FirstName, shipment.Order.ShippingAddress.LastName);
                resulModel.CepDestinatario = shipment.Order.ShippingAddress.ZipPostalCode;
                resulModel.Peso = item.PesoEstimado;
                resulModel.PesoEfetivo = item.PesoEfetivado;
                resulModel.ValorFrete = item.ValorEnvioEstimado;
                resulModel.ValorFreteEfetivado = item.ValorEnvioEfetivado;

                lstPlpAbertaResultModel.Add(resulModel);
            }

            gridModel.Data = lstPlpAbertaResultModel;

            ///Verificar uma forma de apresentar a mensagem de pedidos que não se conseguiu gerar etiqueta
            if (lstPedidosProblema.Count > 0)
            {
                var errorListModel = new List<PLPAbertaResultErrorModel>();

                foreach (var item in lstPedidosProblema)
                {
                    errorListModel.Add(
                        new PLPAbertaResultErrorModel(){ CodigoPedidoErro = item.Id,
                            MensagemErro = string.Format("Serviço selecionado no pedido não encontrado no contrato correios {0}", item.ShippingMethod)
                        }); 
                }

                gridModel.ExtraData = errorListModel;
            }

            return new JsonResult
            {
                Data = gridModel
            };
        }




        [HttpPost, ActionName("PLPAberta")]
        [FormValueRequired("pfp-fechar")]
        public ActionResult PLPFechar(PLPAbertaSearchModel model)
        {

            _sigepWebPlpService.VerificarStatusCartaoPostegem();

            //Fechar a PLP
            PlpSigepWeb plpSigepWeb =_sigepWebPlpService.FecharPlp();


            return PLPAberta();            
        }




        [NonAction]
        private IList<Order> ObterListaPedidos(PLPAbertaSearchModel model)
        {
            var pedidos = new List<int>();


            if (!string.IsNullOrWhiteSpace(model.OrdersIs))
            {
                foreach (var item in model.OrdersIs.Split(','))
                {
                    pedidos.Add(int.Parse(item));
                }
            }

            var orders = _orderService.GetOrdersByIds(pedidos.ToArray());

            return orders;
        }
    }
}
