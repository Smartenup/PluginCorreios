using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Plugin.Shipping.Correios.Models;
using Nop.Plugin.Shipping.Correios.Services;
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
    public class ShippingCorreiosPluginAdminController : Controller
    {


        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;
        private readonly ISigepWebService _sigepWebService;
        private readonly IPdfSigepWebService _pdfSigepWebService;
        private readonly ISigepWebPlpService _sigepWebPlpService;


        public ShippingCorreiosPluginAdminController(
            IOrderService orderService,
            IShipmentService shipmentService,
            ISigepWebService sigepWebService,
            IPdfSigepWebService pdfSigepWebService,
            ISigepWebPlpService sigepWebPlpService)
        {
            _orderService = orderService;
            _shipmentService = shipmentService;
            _sigepWebService = sigepWebService;
            _pdfSigepWebService = pdfSigepWebService;
            _sigepWebPlpService = sigepWebPlpService;
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
            _sigepWebService.UpdateEtiquetaCorreios(shipmentPlp.Etiqueta);

            Shipment shipment = _shipmentService.GetShipmentById(shipmentPlp.ShipmentId);

            ///Excluir o shipment da order
            _shipmentService.DeleteShipment(shipment);

            ///Deletar o envio
            _sigepWebService.DeletePlpShipment(shipmentPlp);

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



        //Método de Fechar a PLP
        //Cria o xml dos pedidos selecionado
        //Solicita a criação do PLP nos correios
        //Salva o xml gerado, o id do correios e atualiza o status para fechado


        public ActionResult PLPAbertaListSelect(DataSourceRequest command, PLPAbertaSearchModel model)
        {
            //Verificar o status do cartão de postagem no SIGEPWEB
            _sigepWebPlpService.VerificarStatusCartaoPostegem();

            //Verificar o cliente no SIGEPWEB
            wsAtendeClienteService.clienteERP cliente = _sigepWebPlpService.ObterClienteSigepWEB();

            //Cria ou atualiza uma PLP em Aberto no Nop
            PlpSigepWeb plpSigebWeb = _sigepWebPlpService.ObterPlpEmAberto();

            IList<Order> lstPedidos = ObterListaPedidos(model);
            IList<Order> lstPedidosProblema = new List<Order>();

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
                    plpSigepWebShipment.PesoEstimado = shipment.TotalWeight.HasValue ? shipment.TotalWeight.Value : 0;
                    plpSigepWebShipment.ValorEnvioEstimado = pedido.OrderShippingInclTax;
                    plpSigepWebShipment.ValorDeclarado = pedido.OrderTotal > CorreiosServices.CONST_VALOR_DECLARADO_MINIMO
                        ? pedido.OrderTotal : CorreiosServices.CONST_VALOR_DECLARADO_MINIMO;

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


            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost, ActionName("PLPAberta")]
        [FormValueRequired("pfp-fechar")]
        public ActionResult PLPFechar(PLPAbertaSearchModel model)
        {

            //Fechar a PLP
            PlpSigepWeb plpSigepWeb =_sigepWebPlpService.FecharPlp();

            //PlpSigepWeb plpSigepWeb = _sigepWebPlpService.ObterPlp(4);

            ///Imprimir o resumo da PLP
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfSigepWebService.PrintFechamentoToPdf(stream, plpSigepWeb);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, string.Format("plp{0}.pdf", plpSigepWeb.PlpSigepWebCorreiosId));
        }



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
