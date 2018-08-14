using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Plugin.Shipping.Correios.Services;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Tasks;
using SmartenUP.Core.Services;
using SmartenUP.Core.Util.Extensions;
using SmartenUP.Core.Util.Helper;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace Nop.Plugin.Shipping.Correios
{
    public class CorreioControleSigepWebTask : ITask
    {
        private readonly ISigepWebService _sigepWebService;
        private readonly ISigepWebPlpService _sigepWebPlpService;
        private readonly IOrderService _orderService;
        private readonly IShippingService _shippingService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IHolidayService _holidayService;

        public CorreioControleSigepWebTask(ISigepWebService sigepWebService,
            ISigepWebPlpService sigepWebPlpService,
            IOrderService orderService,
            IShippingService shippingService,
            ILocalizationService localizationService,
            ILogger logger,
            IHolidayService holidayService)
        {
            _sigepWebService = sigepWebService;
            _sigepWebPlpService = sigepWebPlpService;
            _orderService = orderService;
            _shippingService = shippingService;
            _localizationService = localizationService;
            _logger = logger;
            _holidayService = holidayService;
        }

        public void Execute()
        {
            var brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

            ///Obter as plp fechadas no sistema
            IPagedList<PlpSigepWeb> pagedList = _sigepWebService.ProcurarPlp(PlpSigepWebStatus.Fechada, controleEnvioStatus: PlpSigepWebControleEnvioStatus.Pendente);

            ///Obter os xml das plps nos correios
            foreach (PlpSigepWeb plpFechada in pagedList)
            {
                try
                {
                    plpFechada.PlpSigepWebControleEnvioStatusId = (int)PlpSigepWebControleEnvioStatus.Pendente;

                    string xmlPlpFechada = _sigepWebPlpService.SolicitaXmlPlp(plpFechada.PlpSigepWebCorreiosId.Value);

                    if (string.IsNullOrEmpty(xmlPlpFechada))
                        continue;

                    plpFechada.PlpSigepWebControleEnvioStatusId = (int)PlpSigepWebControleEnvioStatus.Processamento;

                    var document = XDocument.Parse(xmlPlpFechada);
                    ///Obter os valores do envio no xml dos correios
                    foreach (PlpSigepWebShipment shipment in plpFechada.PlpSigepWebShipments)
                    {
                        foreach (XElement objeto in document.Root.Elements("objeto_postal"))
                        {
                            if (objeto.Element("numero_etiqueta").Value.Equals(shipment.Etiqueta.CodigoEtiquetaComVerificador, StringComparison.InvariantCultureIgnoreCase))
                            {
                                try
                                {
                                    shipment.ControleEnvioStatusId = (int)PlpSigepWebControleEnvioStatus.Processamento;

                                    if (!string.IsNullOrWhiteSpace(objeto.Element("peso").Value))
                                        shipment.PesoEfetivado = decimal.Parse(objeto.Element("peso").Value);

                                    if (Decimal.TryParse(objeto.Element("valor_cobrado").Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal valor_cobrado))
                                        shipment.ValorEnvioEfetivado = valor_cobrado;

                                    //Data postagem Sara
                                    if (DateTime.TryParseExact(objeto.Element("data_postagem_sara").Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime postagem))
                                        shipment.DataPostagemSara = TimeZoneInfo.ConvertTimeToUtc(postagem, brasiliaTimeZone);

                                    //Data captacao Sara
                                    if (DateTime.TryParseExact(objeto.Element("data_captacao").Value, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime captacao))
                                        shipment.DataCaptacaoSara = TimeZoneInfo.ConvertTimeToUtc(captacao, brasiliaTimeZone);

                                    //Status processamento Sara
                                    if (!string.IsNullOrWhiteSpace(objeto.Element("status_processamento").Value))
                                        shipment.StatusProcessamentoSara = int.Parse(objeto.Element("status_processamento").Value);
                                    

                                    //Numero_comprovante_postagem
                                    if (!string.IsNullOrWhiteSpace(objeto.Element("numero_comprovante_postagem").Value))
                                        shipment.NumeroComprovantePostagem = long.Parse(objeto.Element("numero_comprovante_postagem").Value);


                                    if (shipment.DataPostagemSara.HasValue)
                                    {
                                        ///Verificar o prazo de entrega pelo calculo de frete
                                        int diasUteis = ObterDiasUteisPrazoCorreios(shipment.OrderId);
                                        ///Quantidade dias úteis
                                        shipment.QuantidadeDiasUteis = diasUteis;
                                        ///Data prevista de entrega pelo sistema
                                        DateTime dataEntrega = shipment.DataPostagemSara.Value.AddWorkDays(diasUteis, _holidayService);
                                        shipment.DataPrevistaEntrega = dataEntrega;

                                        shipment.ControleEnvioStatusId = (int)PlpSigepWebControleEnvioStatus.Concluido;
                                    }

                                    
                                }
                                catch (Exception ex)
                                {
                                    shipment.ControleEnvioStatusId = (int)PlpSigepWebControleEnvioStatus.Erro;

                                    string logError = string.Format("Plugin.Shipping.Correios: Erro atualização status Plp, Ordem {0}",
                                                        shipment.OrderId.ToString());

                                    _logger.Error(logError, ex);
                                }
                            }
                        }
                    }


                    plpFechada.PlpSigepWebControleEnvioStatusId = (int)PlpSigepWebControleEnvioStatus.Concluido;
                }
                catch (Exception ex)
                {
                    if (ex.Message != "Plp ainda não atualizada pelo Sara.")
                        plpFechada.PlpSigepWebControleEnvioStatusId = (int)PlpSigepWebControleEnvioStatus.Erro;

                    string logError = string.Format("Plugin.Shipping.Correios: Erro atualização status Plp {0}", plpFechada.Id.ToString());

                    _logger.Error(logError, ex);

                }
                finally
                {
                    ///Atualiar a plp como processada controle entrega
                    _sigepWebService.UpdatePlp(plpFechada);
                }

            }
        }


        private int ObterDiasUteisPrazoCorreios(int orderId)
        {
            int retorno = 0;

            Order order = _orderService.GetOrderById(orderId);

            ShippingOption shippingOption = _shippingService.GetShippingOption(order);

            retorno = ObterPrazoDescricaoCorreios(shippingOption.Description);

            return retorno;
        }

        private int ObterPrazoDescricaoCorreios(string description)
        {
            int indiceMais = description.LastIndexOf(" + ");

            indiceMais += 3;

            string fraseComPrazo = description.Substring(indiceMais, description.Length - indiceMais);

            string prazoDiasUteisInteiro = NumberHelper.ObterApenasNumeros(fraseComPrazo);

            if (int.TryParse(prazoDiasUteisInteiro, out int retorno))
                return retorno;

            throw new NopException("Não foi possível identificar o prazo de dias uteis no retorno dos correios");
        }

    }
}
