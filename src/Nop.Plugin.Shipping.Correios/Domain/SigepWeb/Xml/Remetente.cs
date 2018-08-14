using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using SmartenUP.Core.Util.Helper;
using System;
using System.Xml.Linq;

namespace Nop.Plugin.Shipping.Correios.Domain.SigepWeb.Xml
{
    public class Remetente
    {

        public XElement ObterRemetente(CorreiosSettings correiosSettings, 
            ShippingSettings shippingSettings, IAddressService addressService)
        {
            string cepOrigem = null;
            string estadoOrigem = null;
            string ruaOrigem = null;
            string cidadeOrigem = null;

            if (shippingSettings.ShippingOriginAddressId > 0)
            {
                var addr = addressService.GetAddressById(shippingSettings.ShippingOriginAddressId);

                if (addr != null && !String.IsNullOrEmpty(addr.ZipPostalCode) && addr.ZipPostalCode.Length == 8)
                {
                    cepOrigem = NumberHelper.ObterApenasNumeros(addr.ZipPostalCode);
                    estadoOrigem = addr.StateProvince.Abbreviation;
                    ruaOrigem = addr.Address1;
                    cidadeOrigem = addr.City;
                }
            }


            var remetente = new XElement("remetente");

            var numeroContrato = new XElement("numero_contrato", correiosSettings.NumeroContratoSIGEP);

            var numeroDiretoria = new XElement("numero_diretoria", correiosSettings.NumeroDiretoria);

            var codigoAdministrativo = new XElement("codigo_administrativo", correiosSettings.CodigoAdministrativoSIGEP);

            var nomeRemetente = new XElement("nome_remetente", new XCData(correiosSettings.NomeRemetenteSIGEP));

            var logradouroRemetente = new XElement("logradouro_remetente", new XCData(correiosSettings.LogradouroRemetenteSIGEP));

            var numeroRemetente = new XElement("numero_remetente", correiosSettings.NumeroRemetenteSIGEP);

            var complementoRemetente = new XElement("complemento_remetente", new XCData(correiosSettings.ComplementoRemetenteSIGEP));

            var bairroRemetente = new XElement("bairro_remetente", new XCData(correiosSettings.BairroRemetenteSIGEP));

            var cepRemetente = new XElement("cep_remetente", cepOrigem);

            var cidadeRemetente = new XElement("cidade_remetente", new XCData(cidadeOrigem));

            var ufRemetente = new XElement("uf_remetente", estadoOrigem);

            var telefoneRemetente = new XElement("telefone_remetente", new XCData(correiosSettings.TelefoneRemetenteSIGEP));

            var faxRemetente = new XElement("fax_remetente");

            var emailRemetente = new XElement("email_remetente", new XCData(correiosSettings.EmailRemetenteSIGEP));


            remetente.Add(numeroContrato);
            remetente.Add(numeroDiretoria);
            remetente.Add(codigoAdministrativo);
            remetente.Add(nomeRemetente);
            remetente.Add(logradouroRemetente);
            remetente.Add(numeroRemetente);
            remetente.Add(complementoRemetente);
            remetente.Add(bairroRemetente);
            remetente.Add(cepRemetente);
            remetente.Add(cidadeRemetente);
            remetente.Add(ufRemetente);
            remetente.Add(telefoneRemetente);
            remetente.Add(faxRemetente);
            remetente.Add(emailRemetente);

            return remetente;

        }
    }
}
