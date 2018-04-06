using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Shipping;
using SmartenUP.Core.Util.Helper;
using System.Globalization;
using System.Xml.Linq;

namespace Nop.Plugin.Shipping.Correios.Domain.SigepWeb.Xml
{
    public class ObjetoPostal
    {
        public XElement ObterObjetoPostal(PlpSigepWebShipment plpShipment, 
            CorreiosSettings correiosSettings,
            IAddressAttributeParser addressAttributeParser,
            IWorkContext workContext,
            IShipmentService shipmentService
            )
        {
            var obtetoPostal = new XElement("objeto_postal");

            var numeroEtiqueta = new XElement("numero_etiqueta");
            numeroEtiqueta.Value = plpShipment.Etiqueta.CodigoEtiquetaComVerificador;

            var codigoObjetoCliente = new XElement("codigo_objeto_cliente");            

            var codigoServicoPostagem = new XElement("codigo_servico_postagem");
            codigoServicoPostagem.Value = plpShipment.Etiqueta.CodigoServico;


            var cubagem = new XElement("cubagem");
            cubagem.Value = "0,0000";
           

            var peso = new XElement("peso");
            if (correiosSettings.UsarPesoPadraoSIGEP)
                peso.Value = NumberHelper.ObterApenasNumerosInteiro(correiosSettings.PesoPadraoSIGEP);
            else
                peso.Value = plpShipment.PesoEstimado.ToString("N0");

            var rt1 = new XElement("rt1");
            var rt2 = new XElement("rt2");
            var dataPostagemSara = new XElement("data_postagem_sara");

            var statusProcessamento = new XElement("status_processamento");
            statusProcessamento.Value = "0";

            var numeroComprovantePostagem = new XElement("numero_comprovante_postagem");
            var valorCobrado = new XElement("valor_cobrado");

            Shipment shipment = shipmentService.GetShipmentById(plpShipment.ShipmentId);

            obtetoPostal.Add(numeroEtiqueta);
            obtetoPostal.Add(codigoObjetoCliente);
            obtetoPostal.Add(codigoServicoPostagem);
            obtetoPostal.Add(cubagem);
            obtetoPostal.Add(peso);
            obtetoPostal.Add(rt1);
            obtetoPostal.Add(rt2);
            obtetoPostal.Add(new Destinatario().ObterDestinatario(addressAttributeParser, workContext, shipment.Order.ShippingAddress));
            obtetoPostal.Add(new Nacional().ObterNacional(shipment.Order.ShippingAddress));
            obtetoPostal.Add(new ServicoAdicional().ObterServicoAdicional(correiosSettings, plpShipment.ValorDeclarado));
            obtetoPostal.Add(new DimensaoObjeto().ObterDimensaoObjeto());
            obtetoPostal.Add(dataPostagemSara);
            obtetoPostal.Add(statusProcessamento);
            obtetoPostal.Add(numeroComprovantePostagem);
            obtetoPostal.Add(valorCobrado);

            return obtetoPostal;
        }
    }

    public class Destinatario
    {

        public XElement ObterDestinatario(IAddressAttributeParser addressAttributeParser,
            IWorkContext workContext, Address address)
        {

            var addressHelper = new AddressHelper(addressAttributeParser, workContext);

            string number = string.Empty;
            string complement = string.Empty;

            addressHelper.GetCustomNumberAndComplement(address.CustomAttributes, out number, out complement);


            var destinatario = new XElement("destinatario");

            string nomeDestinatarioFormatado = string.Format("{0} {1}", address.FirstName, address.LastName);
            nomeDestinatarioFormatado = StringHelper.Formatar(nomeDestinatarioFormatado, 50);
            var nomeDestinatario = new XElement("nome_destinatario", new XCData(nomeDestinatarioFormatado));
            
            var telefoneDestinatario = new XElement("telefone_destinatario", new XCData(AddressHelper.FormatarCelular(address.PhoneNumber)));

            var celularDestinatario = new XElement("celular_destinatario", new XCData(AddressHelper.FormatarCelular(address.PhoneNumber)));

            var emailDestinatario = new XElement("email_destinatario", new XCData(address.Email));

            string logradouroDestinatarioFormatado = StringHelper.Formatar(address.Address1, 50);
            var logradouroDestinatario = new XElement("logradouro_destinatario", new XCData(logradouroDestinatarioFormatado));

            string complementoDestinatarioFormatado = StringHelper.Formatar(complement, 50);
            var complementoDestinatario = new XElement("complemento_destinatario", new XCData(complementoDestinatarioFormatado));

            string numeroFormatado = StringHelper.Formatar(number, 5);
            var numeroEnderecoDestinatario = new XElement("numero_end_destinatario", numeroFormatado);

            destinatario.Add(nomeDestinatario);
            destinatario.Add(telefoneDestinatario);
            destinatario.Add(celularDestinatario);
            destinatario.Add(emailDestinatario);
            destinatario.Add(logradouroDestinatario);
            destinatario.Add(complementoDestinatario);
            destinatario.Add(numeroEnderecoDestinatario);

            return destinatario;
        }
    }

    public class Nacional
    {
        public XElement ObterNacional(Address address)
        {
            var nacional = new XElement("nacional");

            string bairroDestinatarioFormatado = StringHelper.Formatar(address.Address2, 30);
            var bairroDestinatario = new XElement("bairro_destinatario", new XCData(bairroDestinatarioFormatado));

            string cidadeDestinatarioFormatado = StringHelper.Formatar(address.City, 30);
            var cidadeDestinatario = new XElement("cidade_destinatario", new XCData(cidadeDestinatarioFormatado));

            var ufDestinatario = new XElement("uf_destinatario", address.StateProvince.Abbreviation);
            var cepDestinatario = new XElement("cep_destinatario", new XCData(NumberHelper.ObterApenasNumeros(address.ZipPostalCode)));

            var codigoUsuarioPostal = new XElement("codigo_usuario_postal");
            var centroCustoCliente = new XElement("centro_custo_cliente");
            var numeroNotaFiscal = new XElement("numero_nota_fiscal");
            var serieNotaFiscal = new XElement("serie_nota_fiscal");
            var valorNotaFiscal = new XElement("valor_nota_fiscal");
            var naturezaNotaFiscal = new XElement("natureza_nota_fiscal");
            var descricaoObjeto = new XElement("descricao_objeto");
            var valorACobrar = new XElement("valor_a_cobrar");
            valorACobrar.Value = "0,0";

            nacional.Add(bairroDestinatario);
            nacional.Add(cidadeDestinatario);
            nacional.Add(ufDestinatario);
            nacional.Add(cepDestinatario);
            nacional.Add(codigoUsuarioPostal);
            nacional.Add(centroCustoCliente);
            nacional.Add(numeroNotaFiscal);
            nacional.Add(serieNotaFiscal);
            nacional.Add(valorNotaFiscal);
            nacional.Add(naturezaNotaFiscal);
            nacional.Add(descricaoObjeto);
            nacional.Add(valorACobrar);


            return nacional;
        }
    }

    public class ServicoAdicional
    {
        public XElement ObterServicoAdicional(CorreiosSettings correiosSettings, decimal valorDeclarado)
        {
            var servicoAdicional = new XElement("servico_adicional");

            var codigoServicoNacional = new XElement("codigo_servico_adicional");
            codigoServicoNacional.Value = "025";

            servicoAdicional.Add(codigoServicoNacional);

            if (correiosSettings.IncluirValorDeclarado)
            {
                var codigoServicoValorDeclarado = new XElement("codigo_servico_adicional");
                codigoServicoValorDeclarado.Value = "019";
                servicoAdicional.Add(codigoServicoValorDeclarado);

                var valorDeclaradoTag = new XElement("valor_declarado");
                valorDeclaradoTag.Value = valorDeclarado.ToString("N2", CultureInfo.CreateSpecificCulture("pt-BR"));

                servicoAdicional.Add(valorDeclaradoTag);
            }


            if (correiosSettings.IncluirAvisoRecebimento)
            {
                var codigoServicoAvisoRecebimento = new XElement("codigo_servico_adicional");
                codigoServicoAvisoRecebimento.Value = "001";

                servicoAdicional.Add(codigoServicoAvisoRecebimento);
            }

            if (correiosSettings.IncluirMaoPropria)
            {
                var codigoServicoMaoPropria = new XElement("codigo_servico_adicional");
                codigoServicoMaoPropria.Value = "002";

                servicoAdicional.Add(codigoServicoMaoPropria);
            }

            return servicoAdicional;
        }

    }

    public class DimensaoObjeto
    {
        public XElement ObterDimensaoObjeto()
        {
            var dimensaoObjeto = new XElement("dimensao_objeto");

            var tipoObjeto = new XElement("tipo_objeto");
            tipoObjeto.Value = "001";

            var dimensaoAltura = new XElement("dimensao_altura");
            dimensaoAltura.Value = "2";

            var dimensaoLargura = new XElement("dimensao_largura");
            dimensaoLargura.Value = "11";

            var dimensaoComprimento = new XElement("dimensao_comprimento");
            dimensaoComprimento.Value = "16";

            var dimensaoDiamento = new XElement("dimensao_diametro");
            dimensaoDiamento.Value = "0";

            dimensaoObjeto.Add(tipoObjeto);
            dimensaoObjeto.Add(dimensaoAltura);
            dimensaoObjeto.Add(dimensaoLargura);
            dimensaoObjeto.Add(dimensaoComprimento);
            dimensaoObjeto.Add(dimensaoDiamento);


            return dimensaoObjeto;
        }

    }
}
