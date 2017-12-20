using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
using Nop.Plugin.Shipping.Correios.CalcPrecoPrazoWebReference;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Services.Tasks;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using static Nop.Services.Shipping.GetShippingOptionRequest;

namespace Nop.Plugin.Shipping.Correios
{
    /// <summary>
    /// Correios computation method.
    /// </summary>
    public class CorreiosComputationMethod : BasePlugin, IShippingRateComputationMethod , IAdminMenuPlugin
    {

        #region Constants
        private const string CODIGO_SERVICO_PAC_GRANDES_DIMENSOES = "04693";
        private const int MAX_PACKAGE_TOTAL_DIMENSION_PAC_GRANDE = 300;
        private const int MAX_PACKAGE_SIZES_PAC_GRANDE = 150;

        private const int MAX_PACKAGE_WEIGHT = 30;
		private const int MAX_PACKAGE_TOTAL_DIMENSION = 200;
		private const int MAX_PACKAGE_SIZES = 105;

		private const int MIN_PACKAGE_LENGTH = 16;
		private const int MIN_PACKAGE_WIDTH = 11;
		private const int MIN_PACKAGE_HEIGHT = 2;
		private const int MIN_PACKAGE_SIZE = 29;

		private const int MAX_ROLL_TOTAL_DIMENSION = 200;
		private const int MAX_ROLL_LENGTH = 105;
		private const int MAX_ROLL_DIAMETER = 91;

		private const int MIN_ROLL_LENGTH = 18;
		private const int MIN_ROLL_DIAMETER = 5;
		private const int MIN_ROLL_SIZE = 28;

		private const string MEASURE_WEIGHT_SYSTEM_KEYWORD = "kg";
		private const string MEASURE_DIMENSION_SYSTEM_KEYWORD = "millimetres";

        private const int TAMANHO_CEP = 8;
        private const string COMPLEMENTO_FAIXA_CEP_INICIAL = "0";
        private const string COMPLEMENTO_FAIXA_CEP_FINAL = "9";
        
        #endregion

        #region Fields

        private readonly IMeasureService _measureService;
		private readonly IShippingService _shippingService;
		private readonly ISettingService _settingService;
		private readonly CorreiosSettings _correiosSettings;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		private readonly ICurrencyService _currencyService;
		private readonly CurrencySettings _currencySettings;
		private readonly ShippingSettings _shippingSettings;
		private readonly IAddressService _addressService;
		private readonly ILogger _logger;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public CorreiosComputationMethod(IMeasureService measureService,
			IShippingService shippingService, ISettingService settingService,
			CorreiosSettings correiosSettings, IOrderTotalCalculationService orderTotalCalculationService,
			ICurrencyService currencyService, CurrencySettings currencySettings, ShippingSettings shippingSettings, 
            IAddressService addressService, ILogger logger,
            IScheduleTaskService scheduleTaskService,
            ILocalizationService localizationService
            )
		{
            _measureService = measureService;
            _shippingService = shippingService;
            _settingService = settingService;
            _correiosSettings = correiosSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _shippingSettings = shippingSettings;
            _addressService = addressService;
            _logger = logger;
            _scheduleTaskService = scheduleTaskService;
            _localizationService = localizationService;

        }

        #endregion


        #region Properties
        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Realtime;
            }
        }

        public IShipmentTracker ShipmentTracker
        {
            get { return new CorreiosShipmentTracker(_logger, _correiosSettings); }
        }
        #endregion

        #region Methods/// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null)
            {
                response.AddError("Sem items para enviar");
                return response;
            }

            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Endereço de envio em branco");
                return response;
            }

            if (getShippingOptionRequest.ShippingAddress.ZipPostalCode == null)
            {
                response.AddError("CEP de envio em branco");
                return response;
            }

            var result = ProcessShipping(getShippingOptionRequest);

            if (result == null)
            {
                response.AddError("Não há serviços disponíveis no momento");
                return response;
            }

            bool verificarFreteGratisMaisBarato = CheckFreeShippingMaisBarato();
            bool primeiroDaLista = false;

            if (verificarFreteGratisMaisBarato)
            {
                primeiroDaLista = true;
            }

            DeliveryDate biggestDeliveryDate  = GetBiggestDeliveryDate(getShippingOptionRequest.Items);

            var group = new List<string>();

            foreach (cServico servico in result.Servicos.OrderBy(s => decimal.Parse(s.Valor, CultureInfo.GetCultureInfo("pt-BR"))))
            {
                int codigoErro = 0;

                if (Int32.TryParse(servico.Erro, out codigoErro))
                {
                    switch (codigoErro)
                    {
                        case 0:
                        case 9:
                        case 10:
                        case 11:

                            string name = CorreiosServices.GetServicePublicNameById(servico.Codigo.ToString());

                            if (
                                (!group.Contains(name) && !getShippingOptionRequest.IsOrderBasead) ||
                                (getShippingOptionRequest.IsOrderBasead && getShippingOptionRequest.ShippingMethod.Equals(name))
                                )
                            {
                                var option = new ShippingOption();

                                int prazo = (int.Parse(servico.PrazoEntrega) + _correiosSettings.DiasUteisAdicionais);

                                option.Description =  ObterDescricaoPrazo(biggestDeliveryDate, prazo);

                                option.Description += ObterMensagemErro(servico.MsgErro, codigoErro);

                                if (CheckFreeShipping(servico.Codigo, getShippingOptionRequest, primeiroDaLista))
                                {
                                    primeiroDaLista = false;
                                    option.Name = name + " [Frete Grátis]";
                                    option.Rate = 0;
                                    response.ShippingOptions.Insert(0, option);
                                }
                                else
                                {
                                    option.Name = name;

                                    if (!getShippingOptionRequest.IsOrderBasead)
                                    {
                                        option.Rate = decimal.Parse(servico.Valor, CultureInfo.GetCultureInfo("pt-BR")) +
                                        _orderTotalCalculationService.GetShoppingCartAdditionalShippingCharge(getShippingOptionRequest.Items.Select(x => x.ShoppingCartItem).ToList()) +
                                        _correiosSettings.CustoAdicionalEnvio;
                                    }

                                    response.ShippingOptions.Add(option);
                                }

                                group.Add(name);
                            }
                            break;

                        default:

                            string msgError = string.Format("Plugin.Shipping.Correios: erro ao calcular frete: ({0})({1}){2} - CEP {3}",
                                CorreiosServices.GetServiceName(servico.Codigo.ToString()),
                                servico.Erro,
                                servico.MsgErro,
                                getShippingOptionRequest.ShippingAddress.ZipPostalCode);

                            _logger.Error(msgError, exception: null, customer: getShippingOptionRequest.Customer);

                            break;
                    }
                }
                else
                {
                    string msgError = string.Format("Plugin.Shipping.Correios: erro ao calcular frete: ({0})({1}){2} - CEP {3}",
                                CorreiosServices.GetServiceName(servico.Codigo.ToString()),
                                servico.Erro,
                                servico.MsgErro,
                                getShippingOptionRequest.ShippingAddress.ZipPostalCode);

                    _logger.Error(msgError, exception: null, customer: getShippingOptionRequest.Customer);
                }

            }

            return response;
        }


        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request of product for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionProductRequest getShippingOptionProductRequest)
        {
            if (getShippingOptionProductRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            if (getShippingOptionProductRequest.Product == null)
            {
                response.AddError("Sem produto para enviar");
                return response;
            }

            if (getShippingOptionProductRequest.ShippingAddress == null)
            {
                response.AddError("Endereço de envio em branco");
                return response;
            }

            if (getShippingOptionProductRequest.ShippingAddress.ZipPostalCode == null)
            {
                response.AddError("CEP de envio em branco");
                return response;
            }

            var result = ProcessShipping(getShippingOptionProductRequest);

            if (result == null)
            {
                response.AddError("Não há serviços disponíveis no momento");
                return response;
            }

            bool verificarFreteGratisMaisBarato = CheckFreeShippingMaisBarato();
            bool primeiroDaLista = false;

            if (verificarFreteGratisMaisBarato)
            {
                primeiroDaLista = true;
            }

            DeliveryDate biggestDeliveryDate = GetBiggestDeliveryDate(getShippingOptionProductRequest.Product);

            var group = new List<string>();

            foreach (cServico servico in result.Servicos.OrderBy(s => decimal.Parse(s.Valor, CultureInfo.GetCultureInfo("pt-BR"))))
            {
                int codigoErro = 0;

                if (Int32.TryParse(servico.Erro, out codigoErro))
                {
                    switch (codigoErro)
                    {
                        case 0:
                        case 9:
                        case 10:
                        case 11:

                            string name = CorreiosServices.GetServicePublicNameById(servico.Codigo.ToString());

                            if (!group.Contains(name))
                            {
                                var option = new ShippingOption();

                                int prazo = (int.Parse(servico.PrazoEntrega) + _correiosSettings.DiasUteisAdicionais);

                                option.Description = ObterDescricaoPrazo(biggestDeliveryDate, prazo);

                                option.Description += ObterMensagemErro(servico.MsgErro, codigoErro);                                

                                if (CheckFreeShipping(servico.Codigo, getShippingOptionProductRequest, primeiroDaLista))
                                {
                                    primeiroDaLista = false;
                                    option.Name = name + " [Frete Grátis]";
                                    option.Rate = 0;
                                    response.ShippingOptions.Insert(0, option);
                                }
                                else
                                {
                                    option.Name = name;

                                    option.Rate = decimal.Parse(servico.Valor, CultureInfo.GetCultureInfo("pt-BR")) +                                   
                                        _correiosSettings.CustoAdicionalEnvio;

                                    response.ShippingOptions.Add(option);
                                }

                                group.Add(name);
                            }
                            break;

                        default:

                            string msgError = string.Format("Plugin.Shipping.Correios: erro ao calcular frete: ({0})({1}){2} - CEP {3}",
                                CorreiosServices.GetServiceName(servico.Codigo.ToString()),
                                servico.Erro,
                                servico.MsgErro,
                                getShippingOptionProductRequest.ShippingAddress.ZipPostalCode);

                            _logger.Error(msgError, exception: null, customer: getShippingOptionProductRequest.Customer);

                            break;
                    }
                }
                else
                {
                    string msgError = string.Format("Plugin.Shipping.Correios: erro ao calcular frete: ({0})({1}){2} - CEP {3}",
                                CorreiosServices.GetServiceName(servico.Codigo.ToString()),
                                servico.Erro,
                                servico.MsgErro,
                                getShippingOptionProductRequest.ShippingAddress.ZipPostalCode);

                    _logger.Error(msgError, exception: null, customer: getShippingOptionProductRequest.Customer);
                }

            }

            return response;

        }

        private string ObterMensagemErro(string msgErro, int codigoErro)
        {
            if (codigoErro == 11)
            {
                return " O CEP de destino está sujeito a condições especiais de entrega pela ECT e é apresentado com o acréscimo de dias úteis ao prazo regular.";
            }

            if (!string.IsNullOrWhiteSpace(msgErro))
            {
                return " " + msgErro;
            }

            return string.Empty;
        }


        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ShippingCorreios";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Shipping.Correios.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            var settings = new CorreiosSettings()
            {
                Url = "http://ws.correios.com.br/calculador/CalcPrecoPrazo.asmx",
                //CodigoEmpresa = String.Empty,
                //Senha = String.Empty
            };

            _settingService.SaveSetting(settings);

            base.Install();

            ScheduleTask taskByType = _scheduleTaskService.GetTaskByType("Nop.Plugin.Shipping.Correios.CorreioShippingUpdateTask, Nop.Plugin.Shipping.Correios");

            if (taskByType == null)
            {
                taskByType = new ScheduleTask()
                {
                    Enabled = false,
                    Name = "CorreioShippingUpdateTask",
                    Seconds = 600,
                    StopOnError = false,
                    Type = "Nop.Plugin.Shipping.Correios.CorreioShippingUpdateTask, Nop.Plugin.Shipping.Correios"
                };

                _scheduleTaskService.InsertTask(taskByType);
            }
        }

        public override void Uninstall()
        {
            base.Uninstall();

            ScheduleTask taskByType = _scheduleTaskService.GetTaskByType("Nop.Plugin.Shipping.Correios.CorreioShippingUpdateTask, Nop.Plugin.Shipping.Correios");

            if (taskByType != null)
            {
                _scheduleTaskService.DeleteTask(taskByType);
            }
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            /**/
            var siteMap = new XmlSiteMap();
            siteMap.LoadFrom("~/Plugins/Shipping.Correios/sitemap.config");

            //rootNode.ChildNodes.Add(siteMap.RootNode);

            var siteMapNodeRoot = new SiteMapNode();

            siteMapNodeRoot.SystemName = "SigepWeb";
            siteMapNodeRoot.Title =_localizationService.GetResource("Plugins.Shippings.Correios.SIGEPWEB");
            siteMapNodeRoot.IconClass = "fa-tags";
            siteMapNodeRoot.Visible = true;


            var siteMapNodePlpAberta = new SiteMapNode();

            siteMapNodePlpAberta.SystemName = "PlpAberta";
            siteMapNodePlpAberta.Title = _localizationService.GetResource("Plugins.Shippings.Correios.PLPAberta");
            siteMapNodePlpAberta.IconClass = "fa-dot-circle-o";
            siteMapNodePlpAberta.Visible = true;
            siteMapNodePlpAberta.Url = "/admin/Plugins/Shipping/ShippingCorreios/PLPAberta";

            var siteMapNodePlpFechada = new SiteMapNode();

            siteMapNodePlpFechada.SystemName = "PLPFechada";
            siteMapNodePlpFechada.Title = _localizationService.GetResource("Plugins.Shippings.Correios.PLPFechada");
            siteMapNodePlpFechada.IconClass = "fa-dot-circle-o";
            siteMapNodePlpFechada.Visible = true;
            siteMapNodePlpFechada.Url = "/admin/Plugins/Shipping/ShippingCorreios/PLPFechada";

            siteMapNodeRoot.ChildNodes.Add(siteMapNodePlpAberta);
            siteMapNodeRoot.ChildNodes.Add(siteMapNodePlpFechada);

            rootNode.ChildNodes.Add(siteMapNodeRoot);

        }

        #endregion

        #region Utilities


        private string ObterDescricaoPrazo(DeliveryDate biggestDeliveryDate, int prazo)
        {
            if (_correiosSettings.MostrarTempoFabricacao && biggestDeliveryDate != null)
                return ObterDescricaoEnvio(biggestDeliveryDate.GetLocalized(dd => dd.Name)) + " + " + ObterDescricaoPrazo(prazo) + ".";
            else
                return ObterDescricaoPrazo(prazo) + ".";
        }

        private bool CheckFreeShippingMaisBarato()
        {
            if (!_correiosSettings.FreteGratis)
                return false;

            if (_correiosSettings.ServicoFreteGratis.Equals(CorreiosServices.PRIMEIRO_LISTA_MAIS_BARATO))
                return true;

            return false;
        }

        private string ObterDescricaoEnvio(string prazoMaximo)
        {
            string prazoEnvioFabricao = _localizationService.GetResource("Plugins.Shippings.Correios.PrazoEnvioFabricacao");

            //return "Prazo Envio (fabricação) " + prazoMaximo;

            return prazoEnvioFabricao + prazoMaximo;
        }

        private string ObterDescricaoPrazo(int prazo)
        {
            string prazoCorreios = _localizationService.GetResource("Plugins.Shippings.Correios.PrazoCorreios");

            return string.Format(prazoCorreios, prazo);
        }

        private cResultado ProcessShipping(GetShippingOptionRequest getShippingOptionRequest)
		{
			var usedMeasureWeight = _measureService.GetMeasureWeightBySystemKeyword(MEASURE_WEIGHT_SYSTEM_KEYWORD);

			if (usedMeasureWeight == null)
			{
				string e = string.Format("Plugin.Shipping.Correios: Could not load \"{0}\" measure weight", MEASURE_WEIGHT_SYSTEM_KEYWORD);

				_logger.Fatal(e);

				throw new NopException(e);
			}

			var usedMeasureDimension = _measureService.GetMeasureDimensionBySystemKeyword(MEASURE_DIMENSION_SYSTEM_KEYWORD);

			if (usedMeasureDimension == null)
			{
				string e = string.Format("Plugin.Shipping.Correios: Could not load \"{0}\" measure dimension", MEASURE_DIMENSION_SYSTEM_KEYWORD);

				_logger.Fatal(e);

				throw new NopException(e);
			}


			string cepOrigem = null;

			if (_shippingSettings.ShippingOriginAddressId > 0)
			{
				var addr = _addressService.GetAddressById(_shippingSettings.ShippingOriginAddressId);

				if (addr != null && !String.IsNullOrEmpty(addr.ZipPostalCode) && addr.ZipPostalCode.Length >= 8 && addr.ZipPostalCode.Length <= 9)
				{
					cepOrigem = addr.ZipPostalCode;
				}
			}

			if (cepOrigem == null)
			{
				_logger.Fatal("Plugin.Shipping.Correios: CEP de Envio em branco ou inválido, configure nas opções de envio do NopCommerce.Em Administração > Configurações > Configurações de Envio. Formato: 00000000");

				throw new NopException("Plugin.Shipping.Correios: CEP de Envio em branco ou inválido, configure nas opções de envio do NopCommerce.Em Administração > Configurações > Configurações de Envio. Formato: 00000000");
			}
			
            string cepDestino = Regex.Replace(getShippingOptionRequest.ShippingAddress.ZipPostalCode, @"<(.|\n)*?>", " - ");

            cepDestino = cepDestino.Replace("/","");
			

			decimal subtotalBase = decimal.Zero;
            bool includingTax = false;
			decimal orderSubTotalDiscountAmount = decimal.Zero;
			List<Discount> orderSubTotalAppliedDiscount = null;
			decimal subTotalWithoutDiscountBase = decimal.Zero;
			decimal subTotalWithDiscountBase = decimal.Zero;


            if (getShippingOptionRequest.IsOrderBasead)
            {
                orderSubTotalDiscountAmount = getShippingOptionRequest.Order.OrderSubTotalDiscountInclTax;
                subTotalWithoutDiscountBase = getShippingOptionRequest.Order.OrderSubtotalInclTax;
                subTotalWithDiscountBase = getShippingOptionRequest.Order.OrderSubtotalExclTax;
            }
            else
            {
                _orderTotalCalculationService.GetShoppingCartSubTotal(getShippingOptionRequest.Items.Select(x => x.ShoppingCartItem).ToList(), includingTax,
                    out orderSubTotalDiscountAmount, out orderSubTotalAppliedDiscount,
                    out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
            }

            subtotalBase = subTotalWithDiscountBase;

            decimal lengthTmp, widthTmp, heightTmp;

            if (getShippingOptionRequest.IsOrderBasead)
                _shippingService.GetDimensionsByOrder(getShippingOptionRequest.Items, out widthTmp, out lengthTmp, out heightTmp);
            else
                _shippingService.GetDimensions(getShippingOptionRequest.Items, out widthTmp, out lengthTmp, out heightTmp);
            


            int length = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureDimension(lengthTmp, usedMeasureDimension)) / 10);
            int height = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureDimension(heightTmp, usedMeasureDimension)) / 10);
            int width = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureDimension(widthTmp, usedMeasureDimension)) / 10);

            int weight = 0;

            if (getShippingOptionRequest.IsOrderBasead)
                weight = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureWeight(_shippingService.GetTotalWeightByOrder(getShippingOptionRequest), 
                    usedMeasureWeight)));
            else
                weight = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureWeight(_shippingService.GetTotalWeight(getShippingOptionRequest), 
                    usedMeasureWeight)));

            if (length < 1)
				length = 1;

			if (height < 1)
				height = 1;

			if (width < 1)
				width = 1;

			if (weight < 1)
				weight = 1;

			//Altura não pode ser maior que o comprimento, para evitar erro, igualamos e a embalagem deve ser adaptada.
			if (height > length)
			{
				length = height;
			}

			if (IsPackageTooSmall(length, height, width))
			{
				length = MIN_PACKAGE_LENGTH;
				height = MIN_PACKAGE_HEIGHT;
				width = MIN_PACKAGE_WIDTH;
			}

            var correiosCalculation = new CorreiosBatchCalculation(_logger, getShippingOptionRequest.Customer)
            {
                CodigoEmpresa = _correiosSettings.CodigoEmpresa,
                Senha = _correiosSettings.Senha,
                CepOrigem = cepOrigem,
                Servicos = _correiosSettings.CarrierServicesOffered,
                AvisoRecebimento = _correiosSettings.IncluirAvisoRecebimento,
                MaoPropria = _correiosSettings.IncluirMaoPropria,
                CepDestino = cepDestino
            };

            decimal valorDeclarado = 0;

            if (_correiosSettings.IncluirValorDeclarado)
            {
                if (subtotalBase < CorreiosServices.CONST_VALOR_DECLARADO_MINIMO)
                    valorDeclarado = CorreiosServices.CONST_VALOR_DECLARADO_MINIMO;
                else
                    valorDeclarado = subtotalBase;
            }


            if ((!IsPackageTooHeavy(weight)) && (!IsPackageTooLarge(length, height, width, _correiosSettings.CarrierServicesOffered)))
            {
                correiosCalculation.Servicos = GetServiceNotTooHeavyTooLarge(_correiosSettings.CarrierServicesOffered);

                correiosCalculation.Pacotes.Add(new CorreiosBatchCalculation.Pacote()
				{
					Altura = height,
					Comprimento = length,
					Largura = width,
					Diametro = 0,
					FormatoPacote = true,
					Peso = weight,
					ValorDeclarado = valorDeclarado
                });

				return correiosCalculation.Calculate();
			}
			else
			{
                correiosCalculation.Pacotes.Add(new CorreiosBatchCalculation.Pacote()
				{
					Altura = height,
					Comprimento = length,
					Largura = width,
					Diametro = 0,
					FormatoPacote = true,
					Peso = weight,
					ValorDeclarado = valorDeclarado
                });

				var result = correiosCalculation.Calculate();

				return result;
			}
		}

        private cResultado ProcessShipping(GetShippingOptionProductRequest getShippingOptionProductRequest)
        {
            var usedMeasureWeight = _measureService.GetMeasureWeightBySystemKeyword(MEASURE_WEIGHT_SYSTEM_KEYWORD);

            if (usedMeasureWeight == null)
            {
                string e = string.Format("Plugin.Shipping.Correios: Could not load \"{0}\" measure weight", MEASURE_WEIGHT_SYSTEM_KEYWORD);

                _logger.Fatal(e);

                throw new NopException(e);
            }

            var usedMeasureDimension = _measureService.GetMeasureDimensionBySystemKeyword(MEASURE_DIMENSION_SYSTEM_KEYWORD);

            if (usedMeasureDimension == null)
            {
                string e = string.Format("Plugin.Shipping.Correios: Could not load \"{0}\" measure dimension", MEASURE_DIMENSION_SYSTEM_KEYWORD);

                _logger.Fatal(e);

                throw new NopException(e);
            }


            string cepOrigem = null;

            if (_shippingSettings.ShippingOriginAddressId > 0)
            {
                var addr = _addressService.GetAddressById(_shippingSettings.ShippingOriginAddressId);

                if (addr != null && !String.IsNullOrEmpty(addr.ZipPostalCode) && addr.ZipPostalCode.Length >= 8 && addr.ZipPostalCode.Length <= 9)
                {
                    cepOrigem = addr.ZipPostalCode;
                }
            }

            if (cepOrigem == null)
            {
                _logger.Fatal("Plugin.Shipping.Correios: CEP de Envio em branco ou inválido, configure nas opções de envio do NopCommerce.Em Administração > Configurações > Configurações de Envio. Formato: 00000000");

                throw new NopException("Plugin.Shipping.Correios: CEP de Envio em branco ou inválido, configure nas opções de envio do NopCommerce.Em Administração > Configurações > Configurações de Envio. Formato: 00000000");
            }

            string cepDestino = Regex.Replace(getShippingOptionProductRequest.ShippingAddress.ZipPostalCode, @"<(.|\n)*?>", " - ");

            cepDestino = cepDestino.Replace("/", "");


            decimal lengthTmp, widthTmp, heightTmp;

            _shippingService.GetDimensions(getShippingOptionProductRequest.Product, getShippingOptionProductRequest.Quantity, out widthTmp, out lengthTmp, out heightTmp);


            int length = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureDimension(lengthTmp, usedMeasureDimension)) / 10);
            int height = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureDimension(heightTmp, usedMeasureDimension)) / 10);
            int width = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureDimension(widthTmp, usedMeasureDimension)) / 10);

            int weight = 0;

            weight = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureWeight(getShippingOptionProductRequest.Product.Weight, usedMeasureWeight)));
            
            if (length < 1)
                length = 1;

            if (height < 1)
                height = 1;

            if (width < 1)
                width = 1;

            if (weight < 1)
                weight = 1;

            //Altura não pode ser maior que o comprimento, para evitar erro, igualamos e a embalagem deve ser adaptada.
            if (height > length)
            {
                length = height;
            }

            if (IsPackageTooSmall(length, height, width))
            {
                length = MIN_PACKAGE_LENGTH;
                height = MIN_PACKAGE_HEIGHT;
                width = MIN_PACKAGE_WIDTH;
            }

            var correiosCalculation = new CorreiosBatchCalculation(_logger, getShippingOptionProductRequest.Customer)
            {
                CodigoEmpresa = _correiosSettings.CodigoEmpresa,
                Senha = _correiosSettings.Senha,
                CepOrigem = cepOrigem,
                Servicos = _correiosSettings.CarrierServicesOffered,
                AvisoRecebimento = _correiosSettings.IncluirAvisoRecebimento,
                MaoPropria = _correiosSettings.IncluirMaoPropria,
                CepDestino = cepDestino
            };

            if ((!IsPackageTooHeavy(weight)) && (!IsPackageTooLarge(length, height, width, _correiosSettings.CarrierServicesOffered)))
            {
                correiosCalculation.Servicos = GetServiceNotTooHeavyTooLarge(_correiosSettings.CarrierServicesOffered);

                correiosCalculation.Pacotes.Add(new CorreiosBatchCalculation.Pacote()
                {
                    Altura = height,
                    Comprimento = length,
                    Largura = width,
                    Diametro = 0,
                    FormatoPacote = true,
                    Peso = weight,
                    ValorDeclarado = 0
                });

                return correiosCalculation.Calculate();
            }
            else
            {
                correiosCalculation.Pacotes.Add(new CorreiosBatchCalculation.Pacote()
                {
                    Altura = height,
                    Comprimento = length,
                    Largura = width,
                    Diametro = 0,
                    FormatoPacote = true,
                    Peso = weight,
                    ValorDeclarado = 0
                });

                var result = correiosCalculation.Calculate();

                return result;
            }
        }

        private string GetServiceNotTooHeavyTooLarge(string carrierServicesOffered)
        {
            string serviceNotTooHeavyTooLarge = carrierServicesOffered;

            if (carrierServicesOffered.Contains(CODIGO_SERVICO_PAC_GRANDES_DIMENSOES))
            {
                serviceNotTooHeavyTooLarge = carrierServicesOffered.Replace(CODIGO_SERVICO_PAC_GRANDES_DIMENSOES + ",", string.Empty);
            }

            return serviceNotTooHeavyTooLarge;
        }

        private bool IsPackageTooLarge(int length, int height, int width, string carrierServicesOffered)
        {

            int total = TotalPackageSize(length, height, width);

            if (carrierServicesOffered.Contains(CODIGO_SERVICO_PAC_GRANDES_DIMENSOES))
            {
                if (total > MAX_PACKAGE_TOTAL_DIMENSION_PAC_GRANDE || length > MAX_PACKAGE_SIZES_PAC_GRANDE || height > MAX_PACKAGE_SIZES_PAC_GRANDE || width > MAX_PACKAGE_SIZES_PAC_GRANDE)
                    return true;
            }
            else
            {

                if (total > MAX_PACKAGE_TOTAL_DIMENSION || length > MAX_PACKAGE_SIZES || height > MAX_PACKAGE_SIZES || width > MAX_PACKAGE_SIZES)
                    return true;
            }

            return false;

        }

        private bool IsPackageTooSmall(int length, int height, int width)
		{
			int total = TotalPackageSize(length, height, width);

			if (total < MIN_PACKAGE_SIZE || length < MIN_PACKAGE_LENGTH || height < MIN_PACKAGE_HEIGHT || width < MIN_PACKAGE_WIDTH)
				return true;
			else
				return false;
		}

		private int TotalPackageSize(int length, int height, int width)
		{
			return length + width + height;
		}

		private bool IsPackageTooHeavy(int weight)
		{
			if (weight > MAX_PACKAGE_WEIGHT)
				return true;
			else
				return false;
		}

		private bool IsRollTooLarge(int length, int diameter)
		{
			int total = TotalRollSize(length, diameter);

			if (total > MAX_ROLL_TOTAL_DIMENSION || length > MAX_ROLL_LENGTH || diameter > MAX_ROLL_DIAMETER)
				return true;
			else
				return false;
		}

		private bool IsRollTooSmall(int length, int diameter)
		{
			int total = TotalRollSize(length, diameter);

			if (total < MIN_ROLL_SIZE || length < MIN_ROLL_LENGTH || diameter < MIN_ROLL_DIAMETER)
				return true;
			else
				return false;
		}

		private int TotalRollSize(int length, int diameter)
		{
			return length + 2 * diameter;
		}

		private bool IsRollTooHeavy(int weight)
		{
			if (weight > MAX_PACKAGE_WEIGHT)
				return true;
			else
				return false;
		}



        [NonAction]
        private DeliveryDate GetBiggestDeliveryDate(Product product)
        {
            DeliveryDate deliveryDate = null;

            int biggestAmountDays = 0;

            DeliveryDate deliveryDateItem = null;

            deliveryDateItem = _shippingService.GetDeliveryDateById(product.DeliveryDateId);

            string deliveryDateText = deliveryDateItem.GetLocalized(dd => dd.Name);

            int? deliveryBigestInteger = GetBiggestInteger(deliveryDateText);

            if (deliveryBigestInteger.HasValue)
            {
                if (deliveryBigestInteger.Value > biggestAmountDays)
                {
                    biggestAmountDays = deliveryBigestInteger.Value;
                    deliveryDate = deliveryDateItem;
                }
            }

            return deliveryDate;
        }


        [NonAction]
        private DeliveryDate GetBiggestDeliveryDate(IList<PackageItem> Items)
        {
            DeliveryDate deliveryDate = null;

            int biggestAmountDays = 0;

            foreach (var item in Items)
            {
                DeliveryDate deliveryDateItem = null;

                if (item.ShoppingCartItem != null)
                    deliveryDateItem = _shippingService.GetDeliveryDateById(item.ShoppingCartItem.Product.DeliveryDateId); 
                else
                    deliveryDateItem = _shippingService.GetDeliveryDateById(item.OrderItem.Product.DeliveryDateId);

                string deliveryDateText = deliveryDateItem.GetLocalized(dd => dd.Name);

                int? deliveryBigestInteger = GetBiggestInteger(deliveryDateText);

                if (deliveryBigestInteger.HasValue)
                {
                    if (deliveryBigestInteger.Value > biggestAmountDays)
                    {
                        biggestAmountDays = deliveryBigestInteger.Value;
                        deliveryDate = deliveryDateItem;
                    }
                }
            }

            return deliveryDate;
        }
        [NonAction]
        private int? GetBiggestInteger(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var integerResultsList = new List<int>();
            string integerSituation = string.Empty;
            int integerPosition = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (int.TryParse(text[i].ToString(), out integerPosition))
                {
                    integerSituation += text[i].ToString();
                }
                else
                {
                    if (!string.IsNullOrEmpty(integerSituation))
                    {
                        integerResultsList.Add(int.Parse(integerSituation));
                        integerSituation = string.Empty;
                    }
                }
            }

            int integerResult = 0;
            foreach (var item in integerResultsList)
            {
                if (item > integerResult)
                    integerResult = item;
            }


            return integerResult;
        }


        private bool CheckExceptCustomerRoles(Customer customer)
        {
            if (!string.IsNullOrWhiteSpace(_correiosSettings.FreteGratisExcetoCustomerRoleIds))
            { 
                foreach (var role in customer.CustomerRoles)
                {
                    foreach (string id in _correiosSettings.FreteGratisExcetoCustomerRoleIds.Split(';'))
                        if (int.Parse(id) == role.Id)
                            return true;
                }
            }

            return false;
        }


        private bool CheckFreeShipping(int CodigoServico, GetShippingOptionProductRequest getShippingOptionProductRequest, bool primeirodaListaFreeShepping)
        {
            if (!_correiosSettings.FreteGratis)
                return false;

            if (CheckExceptCustomerRoles(getShippingOptionProductRequest.Customer))
                return false;

            if (CodigoServico.ToString().Equals(_correiosSettings.ServicoFreteGratis) || primeirodaListaFreeShepping)
            {
                string cepDestino = Regex.Replace(getShippingOptionProductRequest.ShippingAddress.ZipPostalCode, "[^0-9]", string.Empty);

                string cepInicial = ObterCEPInicial(Regex.Replace(_correiosSettings.CEPInicial, "[^0-9]", string.Empty));

                string cepFinal = ObterCEPFinal(
                                                Regex.Replace(_correiosSettings.CEPInicial, "[^0-9]", string.Empty),
                                                Regex.Replace(_correiosSettings.CEPFinal, "[^0-9]", string.Empty)
                                               );


                if (int.Parse(cepDestino) < int.Parse(cepInicial))
                    return false;

                if (int.Parse(cepDestino) > int.Parse(cepFinal))
                    return false;


                if (_correiosSettings.UtilizaValorMinimo)
                {
                    decimal subtotalBase = decimal.Zero;                  

                    subtotalBase = getShippingOptionProductRequest.Product.Price * getShippingOptionProductRequest.Quantity;

                    if (subtotalBase < _correiosSettings.ValorMinimo)
                        return false;
                }

                return true;
            }

            return false;
        }

        private bool CheckFreeShipping(int CodigoServico, GetShippingOptionRequest getShippingOptionRequest, bool primeirodaListaFreeShepping)
        {
            if (!_correiosSettings.FreteGratis)
                return false;

            if (CheckExceptCustomerRoles(getShippingOptionRequest.Customer))
                return false;

            if (CodigoServico.ToString().Equals(_correiosSettings.ServicoFreteGratis) || primeirodaListaFreeShepping)
            {
                string cepDestino = Regex.Replace(getShippingOptionRequest.ShippingAddress.ZipPostalCode, "[^0-9]", string.Empty);

                string cepInicial = ObterCEPInicial(Regex.Replace(_correiosSettings.CEPInicial, "[^0-9]", string.Empty));

                string cepFinal = ObterCEPFinal(
                                                Regex.Replace(_correiosSettings.CEPInicial, "[^0-9]", string.Empty),
                                                Regex.Replace(_correiosSettings.CEPFinal, "[^0-9]", string.Empty)
                                               );


                if (int.Parse(cepDestino) < int.Parse(cepInicial))
                    return false;

                if (int.Parse(cepDestino) > int.Parse(cepFinal))
                    return false;


                if (_correiosSettings.UtilizaValorMinimo)
                {
                    decimal subtotalBase = decimal.Zero;
                    bool includingTax = false;
                    decimal orderSubTotalDiscountAmount = decimal.Zero;
                    var orderSubTotalAppliedDiscount = new List<Discount>();
                    decimal subTotalWithoutDiscountBase = decimal.Zero;
                    decimal subTotalWithDiscountBase = decimal.Zero;


                    if (getShippingOptionRequest.IsOrderBasead)
                    {
                        orderSubTotalDiscountAmount = getShippingOptionRequest.Order.OrderSubTotalDiscountInclTax;

                        if (getShippingOptionRequest.Order.DiscountUsageHistory.SingleOrDefault() != null)
                            orderSubTotalAppliedDiscount.Add(getShippingOptionRequest.Order.DiscountUsageHistory.SingleOrDefault().Discount);

                        subTotalWithoutDiscountBase = getShippingOptionRequest.Order.OrderSubtotalInclTax;
                        subTotalWithDiscountBase = getShippingOptionRequest.Order.OrderSubTotalDiscountExclTax;
                    }
                    else
                    {
                        _orderTotalCalculationService.GetShoppingCartSubTotal(getShippingOptionRequest.Items.Select(x => x.ShoppingCartItem).ToList(), includingTax,
                            out orderSubTotalDiscountAmount, out orderSubTotalAppliedDiscount,
                            out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
                    }

                    subtotalBase = subTotalWithDiscountBase;

                    if (subtotalBase < _correiosSettings.ValorMinimo)
                    {
                        return false;
                    }


                }

                return true;
            }


            return false;
        }

        private string ObterCEPFinal(string CepInicial, string CepFinal)
        {
            string cepInicial = CepInicial;
            string cepFinal = string.IsNullOrWhiteSpace(CepFinal) ? cepInicial : CepFinal;

            cepFinal = CompletarCEPFinal(cepFinal);

            return cepFinal;
        }

        private string ObterCEPInicial(string CepInicial)
        {
            while (CepInicial.Length != TAMANHO_CEP)
            {
                CepInicial += COMPLEMENTO_FAIXA_CEP_INICIAL;
            }

            return CepInicial;
        }

        private string CompletarCEPFinal(string CepFinal)
        {
            while (CepFinal.Length != TAMANHO_CEP)
            {
                CepFinal += COMPLEMENTO_FAIXA_CEP_FINAL;
            }

            return CepFinal;
        }




        #endregion

    }

}
