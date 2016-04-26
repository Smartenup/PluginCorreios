using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Shipping.Correios.CalcPrecoPrazoWebReference;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace Nop.Plugin.Shipping.Correios
{
	/// <summary>
	/// Correios computation method.
	/// </summary>
	public class CorreiosComputationMethod : BasePlugin, IShippingRateComputationMethod
	{

        private const string CODIGO_SERVICO_PAC_GRANDES_DIMENSOES = "41300";
        private const int MAX_PACKAGE_TOTAL_DIMENSION_PAC_GRANDE = 300;
        private const int MAX_PACKAGE_SIZES_PAC_GRANDE = 150;

        #region Constants
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
		#endregion

		#region Ctor
		public CorreiosComputationMethod(IMeasureService measureService,
			IShippingService shippingService, ISettingService settingService,
			CorreiosSettings correiosSettings, IOrderTotalCalculationService orderTotalCalculationService,
			ICurrencyService currencyService, CurrencySettings currencySettings, ShippingSettings shippingSettings, IAddressService addressService, ILogger logger)
		{
			this._measureService = measureService;
			this._shippingService = shippingService;
			this._settingService = settingService;
			this._correiosSettings = correiosSettings;
			this._orderTotalCalculationService = orderTotalCalculationService;
			this._currencyService = currencyService;
			this._currencySettings = currencySettings;
			this._shippingSettings = shippingSettings;
			this._addressService = addressService;
			this._logger = logger;
		}
		#endregion

		#region Utilities
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

			if (this._shippingSettings.ShippingOriginAddressId > 0)
			{
				var addr = this._addressService.GetAddressById(this._shippingSettings.ShippingOriginAddressId);

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
			Discount orderSubTotalAppliedDiscount = null;
			decimal subTotalWithoutDiscountBase = decimal.Zero;
			decimal subTotalWithDiscountBase = decimal.Zero;


            _orderTotalCalculationService.GetShoppingCartSubTotal(getShippingOptionRequest.Items.Select(x => x.ShoppingCartItem).ToList(), includingTax,
                out orderSubTotalDiscountAmount, out orderSubTotalAppliedDiscount,
				out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

			subtotalBase = subTotalWithDiscountBase;


            decimal lengthTmp, widthTmp, heightTmp;

            _shippingService.GetDimensions(getShippingOptionRequest.Items, out widthTmp, out lengthTmp, out heightTmp);


            int length = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureDimension(lengthTmp, usedMeasureDimension)) / 10);
            int height = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureDimension(heightTmp, usedMeasureDimension)) / 10);
            int width = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureDimension(widthTmp, usedMeasureDimension)) / 10);
            int weight = Convert.ToInt32(Math.Ceiling(_measureService.ConvertFromPrimaryMeasureWeight(_shippingService.GetTotalWeight(getShippingOptionRequest), usedMeasureWeight)));

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

            var correiosCalculation = new CorreiosBatchCalculation(_logger)
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
				Debug.WriteLine("Plugin.Shipping.Correios: Pacote unico");

                correiosCalculation.Servicos = GetServiceNotTooHeavyTooLarge(_correiosSettings.CarrierServicesOffered);

                correiosCalculation.Pacotes.Add(new CorreiosBatchCalculation.Pacote()
				{
					Altura = height,
					Comprimento = length,
					Largura = width,
					Diametro = 0,
					FormatoPacote = true,
					Peso = weight,
					ValorDeclarado = (_correiosSettings.IncluirValorDeclarado ? subtotalBase : 0)
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
					ValorDeclarado = (_correiosSettings.IncluirValorDeclarado ? subtotalBase : 0)
                });

				var result = correiosCalculation.Calculate();

				if (result != null)
				{
					foreach (cServico s in result.Servicos)
					{
						if (s.Erro == "0")
						{
                            /*
							s.Valor = (decimal.Parse(s.Valor, correiosCalculation.PtBrCulture) * totalPackages).ToString(correiosCalculation.PtBrCulture);
							s.ValorAvisoRecebimento = (decimal.Parse(s.ValorAvisoRecebimento, correiosCalculation.PtBrCulture) * totalPackages).ToString(correiosCalculation.PtBrCulture);
							s.ValorMaoPropria = (decimal.Parse(s.ValorMaoPropria, correiosCalculation.PtBrCulture) * totalPackages).ToString(correiosCalculation.PtBrCulture);
							s.ValorValorDeclarado = (decimal.Parse(s.ValorValorDeclarado, correiosCalculation.PtBrCulture) * totalPackages).ToString(correiosCalculation.PtBrCulture);
                            */
						}
					}
				}

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
		#endregion

		#region Methods
		/// <summary>
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
			else
			{
				List<string> group = new List<string>();

				foreach (cServico servico in result.Servicos.OrderBy(s => decimal.Parse(s.Valor, CultureInfo.GetCultureInfo("pt-BR"))))
				{
					Debug.WriteLine("Plugin.Shipping.Correios: Retorno WS");
					Debug.WriteLine("Codigo: " + servico.Codigo);
					Debug.WriteLine("Valor: " + servico.Valor);
					Debug.WriteLine("Valor Mão Própria: " + servico.ValorMaoPropria);
					Debug.WriteLine("Valor Aviso Recebimento: " + servico.ValorAvisoRecebimento);
					Debug.WriteLine("Valor Declarado: " + servico.ValorValorDeclarado);
					Debug.WriteLine("Prazo Entrega: " + servico.PrazoEntrega);
					Debug.WriteLine("Entrega Domiciliar: " + servico.EntregaDomiciliar);
					Debug.WriteLine("Entrega Sabado: " + servico.EntregaSabado);
					Debug.WriteLine("Erro: " + servico.Erro);
					Debug.WriteLine("Msg Erro: " + servico.MsgErro);

                    int codigoErro = 0;

                    if (Int32.TryParse(servico.Erro, out codigoErro))
                    {
                        switch (codigoErro)
                        {
                            case 0:
                            case 10:
                    
						        string name = CorreiosServices.GetServicePublicNameById(servico.Codigo.ToString());

						        if (!group.Contains(name))
						        {
							        ShippingOption option = new ShippingOption();
							        
							        option.Description = "Prazo médio de entrega " + ((int.Parse(servico.PrazoEntrega) + _correiosSettings.DiasUteisAdicionais)).ToString() + " dias úteis";

                                    if (CheckFreeShipping(servico.Codigo, getShippingOptionRequest))
                                    {
                                        option.Name = name + " [Frete Grátis]";
                                        option.Rate = 0;
                                        response.ShippingOptions.Insert(0, option);
                                    }
                                    else
                                    {
                                        option.Name = name;
                                        option.Rate = decimal.Parse(servico.Valor, CultureInfo.GetCultureInfo("pt-BR")) +
                                            _orderTotalCalculationService.GetShoppingCartAdditionalShippingCharge(getShippingOptionRequest.Items.Select(x => x.ShoppingCartItem).ToList()) +
                                            _correiosSettings.CustoAdicionalEnvio;
                                        
                                        response.ShippingOptions.Add(option);
                                    }

							        group.Add(name);
						        }
                                break;

                            default:

                                _logger.Error("Plugin.Shipping.Correios: erro ao calcular frete: (" + CorreiosServices.GetServiceName(servico.Codigo.ToString()) + ")( " + servico.Erro + ") " + servico.MsgErro + " - CEP " + getShippingOptionRequest.ShippingAddress.ZipPostalCode);

                                break;
                        }
                    }
                    else
                    {
                        _logger.Error("Plugin.Shipping.Correios: erro ao calcular frete: (" + CorreiosServices.GetServiceName(servico.Codigo.ToString()) + ")( " + servico.Erro + ") " + servico.MsgErro);
                    }

                }

                return response;
            }
        }

        private bool CheckFreeShipping(int CodigoServico, GetShippingOptionRequest getShippingOptionRequest)
        {
            if(!_correiosSettings.FreteGratis)
                return false;

            if (CodigoServico.ToString().Equals(_correiosSettings.ServicoFreteGratis))
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
                    Discount orderSubTotalAppliedDiscount = null;
                    decimal subTotalWithoutDiscountBase = decimal.Zero;
                    decimal subTotalWithDiscountBase = decimal.Zero;


                    _orderTotalCalculationService.GetShoppingCartSubTotal(getShippingOptionRequest.Items.Select(x => x.ShoppingCartItem).ToList(), includingTax,
                        out orderSubTotalDiscountAmount, out orderSubTotalAppliedDiscount,
                        out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

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
				CodigoEmpresa = String.Empty,
				Senha = String.Empty
			};

			_settingService.SaveSetting(settings);

			base.Install();
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
		#endregion

		public Services.Shipping.Tracking.IShipmentTracker ShipmentTracker
		{
			get { return new CorreiosShipmentTracker(this._logger, this._correiosSettings); }
		}
	}
}
