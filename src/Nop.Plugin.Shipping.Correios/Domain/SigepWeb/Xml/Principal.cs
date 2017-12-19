using Nop.Core;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Shipping;
using System;
using System.Xml.Linq;

namespace Nop.Plugin.Shipping.Correios.Domain.SigepWeb.Xml
{
    public class Principal
    {
        public XDocument ObterXmlPrincipal(PlpSigepWeb plpSigebWeb, 
            CorreiosSettings correiosSettings, ShippingSettings shippingSettings,
            IAddressService addressService, IAddressAttributeParser addressAttributeParser,
            IWorkContext workContext, IShipmentService shipmentService)
        {
            var principal = new XDocument();

            principal.Declaration = new XDeclaration("1.0", "ISO-8859-1", String.Empty);

            var root = new XElement("correioslog");

            var tipoArquivo = new XElement("tipo_arquivo");
            tipoArquivo.Value = "Postagem";

            var versaoArquivo = new XElement("versao_arquivo");
            versaoArquivo.Value = "2.3";

            var formaPagamento = new XElement("forma_pagamento");

            root.Add(tipoArquivo);
            root.Add(versaoArquivo);

            root.Add(new PLP().ObterXmlPLP(correiosSettings.CartaoPostagemSIGEP));

            root.Add(new Remetente().ObterRemetente(correiosSettings, shippingSettings, addressService));

            root.Add(formaPagamento);

            foreach (var plpShipments in plpSigebWeb.PlpSigepWebShipments)
            {
                root.Add(new ObjetoPostal().ObterObjetoPostal(plpShipments, correiosSettings, addressAttributeParser, workContext, shipmentService));
            }


            principal.Add(root);

            return principal;
        }
    }


    internal class PLP
    {
        public XElement ObterXmlPLP(string cartaoPostagem)
        {
            var plp = new XElement("plp");

            var idPlp = new XElement("id_plp");
            var valorGlobal = new XElement("valor_global");
            var mcuUnidadePostagem = new XElement("mcu_unidade_postagem");
            var nomeUnidadePostagem = new XElement("nome_unidade_postagem");

            var cartao_postagem = new XElement("cartao_postagem");
            cartao_postagem.Value = cartaoPostagem;

            plp.Add(idPlp);
            plp.Add(valorGlobal);
            plp.Add(mcuUnidadePostagem);
            plp.Add(nomeUnidadePostagem);
            plp.Add(cartao_postagem);

            return plp;
        }

    }
}
