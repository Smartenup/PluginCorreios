﻿using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Plugin.Shipping.Correios.Models;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using System;
using System.ServiceModel;
using System.Text;
using System.Web.Mvc;

namespace Nop.Plugin.Shipping.Correios.Controllers
{
    [AdminAuthorize]
    public class ShippingCorreiosController : Controller
	{
		private readonly CorreiosSettings _correiosSettings;
		private readonly ISettingService _settingService;
		private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;

        public ShippingCorreiosController(CorreiosSettings correiosSettings, 
            ISettingService settingService, 
            ILocalizationService localizationService,
            ICustomerService customerService
            )
		{
            _customerService = customerService;
            _correiosSettings = correiosSettings;
            _settingService = settingService;
            _localizationService = localizationService;
        }


        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new CorreiosShippingModel();

            model.Url = _correiosSettings.Url;
            model.CodigoEmpresa = _correiosSettings.CodigoEmpresa;
            model.Senha = _correiosSettings.Senha;
            model.CustoAdicionalEnvio = _correiosSettings.CustoAdicionalEnvio.ToString();
            model.IncluirAvisoRecebimento = _correiosSettings.IncluirAvisoRecebimento;
            model.IncluirMaoPropria = _correiosSettings.IncluirMaoPropria;
            model.IncluirValorDeclarado = _correiosSettings.IncluirValorDeclarado;
            model.DiasUteisAdicionais = _correiosSettings.DiasUteisAdicionais;

            var services = new CorreiosServices();

            model.AvailableCarrierServicesList.Add(new SelectListItem()
            {
                Text = "Primeiro da lista ou o serviço mais barato do momento",
                Value = CorreiosServices.PRIMEIRO_LISTA_MAIS_BARATO
            }
            );

            // Load service names
            foreach (string service in services.Services)
            {
                model.AvailableCarrierServices.Add(service);
                model.AvailableCarrierServicesList.Add(new SelectListItem()
                {
                    Text = CorreiosServices.GetServiceId(service) + " - " + service,
                    Value = CorreiosServices.GetServiceId(service)
                }
                );
            }

            string carrierServicesOfferedDomestic = _correiosSettings.CarrierServicesOffered;

            if (!String.IsNullOrEmpty(carrierServicesOfferedDomestic))
            {
                foreach (string service in services.Services)
                {
                    string serviceId = CorreiosServices.GetServiceId(service);
                    if (!String.IsNullOrEmpty(serviceId) && !String.IsNullOrEmpty(carrierServicesOfferedDomestic))
                    {
                        if (carrierServicesOfferedDomestic.Contains(serviceId))
                            model.CarrierServicesOffered.Add(service);
                    }
                }
            }



            model.MostrarTempoFabricacao = _correiosSettings.MostrarTempoFabricacao;
            model.FreteGratis = _correiosSettings.FreteGratis;
            model.CEPInicial = _correiosSettings.CEPInicial;
            model.CEPFinal = _correiosSettings.CEPFinal;
            model.UtilizaValorMinimo = _correiosSettings.UtilizaValorMinimo;
            model.ValorMinimo = _correiosSettings.ValorMinimo;
            model.ServicoFreteGratis = _correiosSettings.ServicoFreteGratis;
            model.UsuarioServicoRastreamento = _correiosSettings.UsuarioServicoRastreamento;
            model.SenhaServicoRastreamento = _correiosSettings.SenhaServicoRastreamento;


            if(!string.IsNullOrWhiteSpace(_correiosSettings.FreteGratisExcetoCustomerRoleIds))
                foreach (string id in _correiosSettings.FreteGratisExcetoCustomerRoleIds.Split(';')) 
                    model.SelectedCustomerRoleIds.Add(int.Parse(id));

            var allRoles = _customerService.GetAllCustomerRoles(true);

            foreach (var role in allRoles)
            {
                model.AvailableCustomerRoles.Add(new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Id.ToString(),
                    Selected = model.SelectedCustomerRoleIds.Contains(role.Id)
                });
            }

            return View("~/Plugins/Shipping.Correios/Views/ShippingCorreios/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        [AdminAntiForgery]
        [ValidateInput(false)]
        public ActionResult Configure(CorreiosShippingModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            //save settings
            _correiosSettings.Url = model.Url;
            _correiosSettings.CodigoEmpresa = model.CodigoEmpresa;
            _correiosSettings.Senha = model.Senha;
            _correiosSettings.CustoAdicionalEnvio = Convert.ToDecimal(!string.IsNullOrEmpty(model.CustoAdicionalEnvio) ? model.CustoAdicionalEnvio : "0.0");
            _correiosSettings.IncluirAvisoRecebimento = model.IncluirAvisoRecebimento;
            _correiosSettings.IncluirMaoPropria = model.IncluirMaoPropria;
            _correiosSettings.IncluirValorDeclarado = model.IncluirValorDeclarado;
            _correiosSettings.DiasUteisAdicionais = model.DiasUteisAdicionais;
            // Save selected services
            var carrierServicesOfferedDomestic = new StringBuilder();
            int carrierServicesDomesticSelectedCount = 0;
            if (model.CheckedCarrierServices != null)
            {
                foreach (var cs in model.CheckedCarrierServices)
                {
                    carrierServicesDomesticSelectedCount++;
                    string serviceId = CorreiosServices.GetServiceId(cs);
                    if (!String.IsNullOrEmpty(serviceId))
                        carrierServicesOfferedDomestic.AppendFormat("{0},", serviceId);
                }
            }

            if (carrierServicesDomesticSelectedCount == 0)
                _correiosSettings.CarrierServicesOffered = "41106,40010,40215";
            else
                _correiosSettings.CarrierServicesOffered = carrierServicesOfferedDomestic.ToString().TrimEnd(',');


            _correiosSettings.MostrarTempoFabricacao = model.MostrarTempoFabricacao;
            _correiosSettings.FreteGratis = model.FreteGratis;
            _correiosSettings.CEPInicial = model.CEPInicial;
            _correiosSettings.CEPFinal = model.CEPFinal;
            _correiosSettings.UtilizaValorMinimo = model.UtilizaValorMinimo;
            _correiosSettings.ValorMinimo = model.ValorMinimo;
            _correiosSettings.ServicoFreteGratis = model.ServicoFreteGratis;
            _correiosSettings.UsuarioServicoRastreamento = model.UsuarioServicoRastreamento;
            _correiosSettings.SenhaServicoRastreamento = model.SenhaServicoRastreamento;

            string selectedCustomerRoleIds = string.Empty;

            foreach (int item in model.SelectedCustomerRoleIds)
            {
                selectedCustomerRoleIds += item.ToString() + ';';
            }

            if (!string.IsNullOrWhiteSpace(selectedCustomerRoleIds))
            {
                if (selectedCustomerRoleIds.LastIndexOf(';') == selectedCustomerRoleIds.Length - 1)
                {
                    selectedCustomerRoleIds = selectedCustomerRoleIds.Substring(0, selectedCustomerRoleIds.Length - 1);
                }
            }

            _correiosSettings.FreteGratisExcetoCustomerRoleIds = selectedCustomerRoleIds;


            _settingService.SaveSetting(_correiosSettings);

            //ViewData["sucesso"] = _localizationService.GetResource("Admin.Configuration.Updated");

            return Configure();
        }


        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAddresByCEP(string cep)
        {
            //this action method gets called via an ajax request
            if (String.IsNullOrEmpty(cep))
                throw new ArgumentNullException("cep");

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);

            var address = new EndpointAddress("https://apps.correios.com.br/SigepMasterJPA/AtendeClienteService/AtendeCliente?wsdl");

            wsAtendeClienteService.AtendeClienteClient ws = new wsAtendeClienteService.AtendeClienteClient(binding, address);

            wsAtendeClienteService.enderecoERP dados = ws.consultaCEP(cep);

            return Json(dados, JsonRequestBehavior.AllowGet);
        }


    }
}
