using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Plugin.Shipping.Correios.Util;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Shipping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public class PdfSigepWebService : IPdfSigepWebService
    {

        private const int PHONE_WITHOUT_AREA_CODE_MAX_LENGTH = 9;
        private const int PHONE_ONLY_NUMBER_ONE_MAX_LENGTH = 11;


        private readonly PdfSettings _pdfSettings;
        private readonly ISettingService _settingContext;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly IShipmentService _shipmentService;
        private readonly ShippingSettings _shippingSettings;
        private readonly IAddressService _addressService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly CorreiosSettings _correiosSettings;

        public PdfSigepWebService(PdfSettings pdfSettings, 
            ISettingService settingContext,
            ILanguageService languageService,
            IWorkContext workContext,
            ShippingSettings shippingSettings,
            IAddressService addressService,
            CorreiosSettings correiosSettings,
            IShipmentService shipmentService,
            IAddressAttributeParser addressAttributeParser,
            IPictureService pictureService
            )
        {
            _pdfSettings = pdfSettings;
            _settingContext = settingContext;
            _languageService = languageService;
            _workContext = workContext;
            _shippingSettings = shippingSettings;
            _addressService = addressService;
            _correiosSettings = correiosSettings;
            _shipmentService = shipmentService;
            _addressAttributeParser = addressAttributeParser;
            _pictureService = pictureService;
        }


        public void PrintFechamentoToPdf(Stream stream, PlpSigepWeb plpSigepWeb)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (plpSigepWeb == null)
                throw new ArgumentNullException("plpSigepWeb");

            var pointsMargem = Utilities.MillimetersToPoints(10f);

            var doc = new Document(PageSize.A4, pointsMargem, pointsMargem, pointsMargem, pointsMargem);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);

            doc.Open();

            var tableMaster = new PdfPTable(1);
            var pointsValue = Utilities.MillimetersToPoints(212.72f);

            tableMaster.TotalWidth = pointsValue;
            //fix the absolute width of the table
            tableMaster.LockedWidth = true;
            tableMaster.DefaultCell.Border = Rectangle.NO_BORDER;

            ///Três cabeçalho (Logo Correios e Nome da empresa Correios) que se repete na 2ª via
            tableMaster.AddCell(ObterCabecalhoPlpFechamento());
            ///Tabela da PLP ( Tabela com Titulo, dados da empresa com contrato, numero da PLP e detalhes dos serviços utilizados) que se repete na 2ª via
            tableMaster.AddCell(ObterTabelaPlpFechamento(plpSigepWeb, pdfWriter));
            ///Linha escrito primeira via em cima, Linha com o local para picotar inicio ao final da tabela, e 2º via na linha abaixo
            tableMaster.AddCell(ObterTabelaLinhaPicotada());
            ///Repetição do cabeçalho
            tableMaster.AddCell(ObterCabecalhoPlpFechamento());
            ///Repetição tabela PLP
            tableMaster.AddCell(ObterTabelaPlpFechamento(plpSigepWeb, pdfWriter));

            doc.Add(tableMaster);

            doc.Close();

        }

        private PdfPTable ObterTabelaLinhaPicotada()
        {
            var arial9 = FontFactory.GetFont("Arial", 9, Font.NORMAL);
            var pointsValue = Utilities.MillimetersToPoints(212);


            var fechamentoTable = new PdfPTable(1);
            fechamentoTable.TotalWidth = pointsValue;

            var viaCorreiosCell = new PdfPCell(new Phrase("1ª Via Correios", arial9));
            viaCorreiosCell.Colspan = 2;
            viaCorreiosCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            viaCorreiosCell.Border = Rectangle.BOTTOM_BORDER;

            fechamentoTable.AddCell(viaCorreiosCell);

            var viaClienteCell = new PdfPCell(new Phrase("2ª Via Cliente", arial9));
            viaClienteCell.Colspan = 2;
            viaClienteCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            viaClienteCell.Border = Rectangle.TOP_BORDER;

            fechamentoTable.AddCell(viaClienteCell);


            PdfPCell blankCell = new PdfPCell(new Phrase(Chunk.NEWLINE));
            blankCell.Border = PdfPCell.NO_BORDER;
            blankCell.Colspan = 3;
            fechamentoTable.AddCell(blankCell);

            return fechamentoTable;
        }

        private PdfPTable ObterTabelaPlpFechamento(PlpSigepWeb plpSigelWeb, PdfWriter pdfWriter)
        {

            var arial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.BLACK);
            var arial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.BLACK);
            var arial9 = FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK);
            var pointsValue = Utilities.MillimetersToPoints(200);

            var fechamentoTable = new PdfPTable(3);            
            fechamentoTable.TotalWidth = pointsValue;

            fechamentoTable.SetWidths(new[] { 0.33f, 0.33f, 0.33f });

            ///*Primeira Linha Colspan 3 - Cabecalho*/
            var tituloCell = new PdfPCell(new Phrase("PRÉ - LISTA DE POSTAGEM - PLP SIGEP WEB", arial10Bold));
            tituloCell.Colspan = 3;
            tituloCell.HorizontalAlignment = Element.ALIGN_CENTER;
            tituloCell.Border = Rectangle.BOX;
            fechamentoTable.AddCell(tituloCell);


            ///*Segunda Linha - tabela dos dados do contrato e numero da plp*/
            var sigepWebCell = new PdfPCell(new Phrase("SIGEP WEB - Gerenciador de Postagens dos Correios", arial9Bold));
            sigepWebCell.Colspan = 2;
            sigepWebCell.HorizontalAlignment = Element.ALIGN_LEFT;
            sigepWebCell.Border = Rectangle.TOP_BORDER;
            fechamentoTable.AddCell(sigepWebCell);

            var numeroPlpCell = new PdfPCell(new Phrase(string.Concat("NºPLP: ", plpSigelWeb.PlpSigepWebCorreiosId) , arial9Bold));
            numeroPlpCell.HorizontalAlignment = Element.ALIGN_CENTER;
            numeroPlpCell.Border = Rectangle.TOP_BORDER;
            fechamentoTable.AddCell(numeroPlpCell);


            
            ///*Terceira linha - numero contrato *
            var contratoCell = new PdfPCell(new Phrase("Contrato: ", arial9Bold));
            contratoCell.Phrase.Add(new Phrase(_correiosSettings.NumeroContratoSIGEP, arial9));
            contratoCell.HorizontalAlignment = Element.ALIGN_LEFT;
            contratoCell.Border = Rectangle.NO_BORDER;
            contratoCell.Colspan = 3;
            fechamentoTable.AddCell(contratoCell);

            
            ///*Quarta Linha   - nome do cliente e código de barras da PLP (rowspan 3)*
            var nomeClienteCell = new PdfPCell(new Phrase("Cliente: ", arial9Bold));
            nomeClienteCell.Phrase.Add(new Phrase(_correiosSettings.NomeRemetenteSIGEP, arial9));
            nomeClienteCell.HorizontalAlignment = Element.ALIGN_LEFT;
            nomeClienteCell.Border = Rectangle.NO_BORDER;
            nomeClienteCell.Colspan = 2;


            string barCode = NumberHelper.ObterApenasNumeros(plpSigelWeb.PlpSigepWebCorreiosId.ToString());
            var codigoBarrasPlpImg = ImageHelper.GetBarCode(barCode, pdfWriter, 2.0f);

            var codigoBarrasPlpCell = new PdfPCell();
            codigoBarrasPlpCell.Rowspan = 3;
            codigoBarrasPlpCell.Border = Rectangle.NO_BORDER;
            codigoBarrasPlpCell.AddElement(codigoBarrasPlpImg);

            fechamentoTable.AddCell(nomeClienteCell);
            fechamentoTable.AddCell(codigoBarrasPlpCell);
            
            ///*Quinta Linha   - telefone de contato *
            var telefoneCell = new PdfPCell(new Phrase("Telefone de contato: ", arial9Bold));
            telefoneCell.Phrase.Add(new Phrase(_correiosSettings.TelefoneRemetenteSIGEP, arial9));
            telefoneCell.HorizontalAlignment = Element.ALIGN_LEFT;
            telefoneCell.Border = Rectangle.NO_BORDER;
            telefoneCell.Colspan = 2;

            fechamentoTable.AddCell(telefoneCell);

            
            ///*Sexta Linha   - e-mail para contato *
            var emailCell = new PdfPCell(new Phrase("Email de contato: ", arial9Bold));
            emailCell.Phrase.Add(new Phrase(_correiosSettings.EmailRemetenteSIGEP, arial9));
            emailCell.HorizontalAlignment = Element.ALIGN_LEFT;
            emailCell.Border = Rectangle.BOTTOM_BORDER;
            emailCell.Colspan = 2;
            fechamentoTable.AddCell(emailCell);


            ///*Setima Linha   - Quantidade e Serviço título*
            var emailQuantidadeTituloCell = new PdfPCell(new Phrase("Quantidade:", arial9Bold));
            emailQuantidadeTituloCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            emailQuantidadeTituloCell.Border = Rectangle.TOP_BORDER;
            fechamentoTable.AddCell(emailQuantidadeTituloCell);

            var emailServicoTituloCell = new PdfPCell(new Phrase("Serviço:", arial9Bold));
            emailServicoTituloCell.HorizontalAlignment = Element.ALIGN_LEFT;
            emailServicoTituloCell.Border = Rectangle.TOP_BORDER;
            emailServicoTituloCell.Colspan = 2;
            fechamentoTable.AddCell(emailServicoTituloCell);

            ///*Oitava Linha ou mais - Quantidade e Serviço valor*
            var agrupamento = plpSigelWeb.PlpSigepWebShipments.
                GroupBy(c => c.Etiqueta.CodigoServico).
                Select(group => new { CodigoServico = group.Key, QuantidadeServico = group.Count() });

            int total = 0;

            foreach (var item in agrupamento)
            {
                total += item.QuantidadeServico;

                var quantidadeValorCell = new PdfPCell(new Phrase(item.QuantidadeServico.ToString(), arial9Bold));
                quantidadeValorCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                quantidadeValorCell.Border = Rectangle.NO_BORDER;
                fechamentoTable.AddCell(quantidadeValorCell);

                string nomeServico = string.Format("{0} - {1}", item.CodigoServico, CorreiosServices.GetServiceName(item.CodigoServico));

                var servicoValorCell = new PdfPCell(new Phrase(nomeServico, arial9));
                servicoValorCell.HorizontalAlignment = Element.ALIGN_LEFT;
                servicoValorCell.Border = Rectangle.NO_BORDER;
                servicoValorCell.Colspan = 2;
                fechamentoTable.AddCell(servicoValorCell);
            }


            ///*Linha de total de quantidade  e data de entrega
            var totalCell = new PdfPCell(new Phrase(string.Concat("Total: ", total), arial9Bold));
            totalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalCell.Border = Rectangle.NO_BORDER;
            fechamentoTable.AddCell(totalCell);

            var emBrancoCell = new PdfPCell();
            emBrancoCell.Border = Rectangle.NO_BORDER;
            fechamentoTable.AddCell(emBrancoCell);

            var dataEntregaCell = new PdfPCell(new Phrase("Data da Entrega:____/____/______", arial9Bold));
            dataEntregaCell.HorizontalAlignment = Element.ALIGN_CENTER;
            dataEntregaCell.Border = Rectangle.NO_BORDER;
            fechamentoTable.AddCell(dataEntregaCell);


            ///espaço para assinatura
            PdfPCell blankCell = new PdfPCell(new Phrase(Chunk.NEWLINE));
            blankCell.Border = PdfPCell.NO_BORDER;
            blankCell.Colspan = 3;
            fechamentoTable.AddCell(blankCell);

            ///Linha de assinatura
            var linhaAssinaturaCell = new PdfPCell(new Phrase("________________________________________________", arial9Bold));
            linhaAssinaturaCell.Colspan = 3;
            linhaAssinaturaCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            linhaAssinaturaCell.Border = Rectangle.NO_BORDER;
            fechamentoTable.AddCell(linhaAssinaturaCell);

            ///Linha de display de assinatura e matricula correios
            fechamentoTable.AddCell(emBrancoCell);
            fechamentoTable.AddCell(emBrancoCell);

            var linhaAssinaturaMatriculaCell = new PdfPCell(new Phrase("Assinatura / Matrícula dos Correios", arial9Bold));
            linhaAssinaturaMatriculaCell.HorizontalAlignment = Element.ALIGN_CENTER;
            linhaAssinaturaMatriculaCell.Border = Rectangle.NO_BORDER;
            fechamentoTable.AddCell(linhaAssinaturaMatriculaCell);

            ///espaço para fechamento da tabela
            fechamentoTable.AddCell(blankCell);

            return fechamentoTable;

        }

        private PdfPTable ObterCabecalhoPlpFechamento()
        {
            var pointsValue = Utilities.MillimetersToPoints(200f);
            var pointsValue20 = Utilities.MillimetersToPoints(50f);
            var pointsValue50 = Utilities.MillimetersToPoints(50f);

            string pathImages = ImageHelper.ObterPathImagens();

            var etiquetaTable = new PdfPTable(2);

            etiquetaTable.TotalWidth = pointsValue;
            etiquetaTable.LockedWidth = true;
            etiquetaTable.DefaultCell.Border = Rectangle.NO_BORDER;

            etiquetaTable.SetWidths(new[] { 0.25f, 0.75f });

            var logoCorreios = Image.GetInstance(new Uri(Path.Combine(pathImages, "logo_correios_fechamento.png")));
            logoCorreios.ScaleToFit(pointsValue20, pointsValue50);

            var cellLogo = new PdfPCell();
            cellLogo.Border = Rectangle.NO_BORDER;
            cellLogo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellLogo.AddElement(logoCorreios);
            etiquetaTable.AddCell(cellLogo);

            var cellCabecalho = new PdfPCell(new Phrase("EMPRESA BRASILEIRA DE CORREIOS E TELEGRAFOS", FontFactory.GetFont("Arial", 14, Font.BOLD)));
            cellCabecalho.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellCabecalho.VerticalAlignment = Element.ALIGN_MIDDLE;
            cellCabecalho.Border = Rectangle.NO_BORDER;
            etiquetaTable.AddCell(cellCabecalho);
            

            return etiquetaTable;

        }

        public void PrintEtiquetasToPdf(Stream stream, IList<PlpSigepWebShipment> sigepWebShipments)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (sigepWebShipments == null)
                throw new ArgumentNullException("sigepWebShipments");

            var pointsMargem = Utilities.MillimetersToPoints(1.6f);

            var doc = new Document(PageSize.LETTER, pointsMargem, pointsMargem, pointsMargem, pointsMargem);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);

            doc.Open();

            //fonts
            var tableMaster = new PdfPTable(2);
            var pointsValue = Utilities.MillimetersToPoints(212.72f);

            tableMaster.TotalWidth = pointsValue;
            //fix the absolute width of the table
            tableMaster.LockedWidth = true;
            tableMaster.DefaultCell.Border = Rectangle.NO_BORDER;

            int ordNum = 0;

            foreach (var plpShipment in sigepWebShipments)
            {
                tableMaster.AddCell(ObterEtiqueta(plpShipment, pdfWriter));
                ordNum++;
            }

            if (ordNum %2 != 0)
                tableMaster.AddCell(ObterEtiquetaBranco());

            doc.Add(tableMaster);

            doc.Close();
        }


        private PdfPTable ObterEtiquetaBranco()
        {
            var etiquetaTable = new PdfPTable(3);
            //Quatro colunas - 1 Margem

            var pointsValue = Utilities.MillimetersToPoints(106f);

            etiquetaTable.TotalWidth = pointsValue;
            //fix the absolute width of the table
            etiquetaTable.LockedWidth = true;

            etiquetaTable.SetWidths(new[] { 0.33f, 0.33f, 0.33f});


            etiquetaTable.DefaultCell.Border = Rectangle.NO_BORDER;

            return etiquetaTable;

        }

        private PdfPTable ObterEtiqueta(PlpSigepWebShipment plpShipment, PdfWriter pdfWriter)
        {
            string cepOrigem = null;
            string estadoOrigem = null;
            string cidadeOrigem = null;

            if (_shippingSettings.ShippingOriginAddressId > 0)
            {
                var addr = _addressService.GetAddressById(_shippingSettings.ShippingOriginAddressId);

                if (addr != null && !String.IsNullOrEmpty(addr.ZipPostalCode) && addr.ZipPostalCode.Length == 8 )
                {
                    cepOrigem = NumberHelper.ObterApenasNumeros(addr.ZipPostalCode);
                    estadoOrigem = addr.StateProvince.Abbreviation;
                    cidadeOrigem = addr.City;
                }
            }


            Shipment shipment = _shipmentService.GetShipmentById(plpShipment.ShipmentId);

            var number = string.Empty;
            var complement = string.Empty;

            new AddressHelper(_addressAttributeParser, _workContext).GetCustomNumberAndComplement(shipment.Order.ShippingAddress.CustomAttributes,
                out number, out complement);

            string cepDestino = NumberHelper.ObterApenasNumeros(shipment.Order.ShippingAddress.ZipPostalCode);

            /*  Campo                                           - Caracteres
             *  [1]CEP destino                                     - 8 
             *  [2]Complemento do CEP                              - 5
             *  [3]CEP Origem                                      - 8
             *  [4]Complemento do CEP                              - 5
             *  [5]Validador do CEP Destino                        - 1
             *  [6]IDV                                             - 2 ///51 - Encomenda | 81 - Malotes
             *  [7]Etiqueta                                        - 13
             *  [8]Serviços Adicionais (25, 01, 02, 19, 49, 57)    - 12
             *  [9]Cartão de Postagem                              - 10
             *  [10]Código do Serviço                               - 5
             *  [11]Informação de Agrupamento                       - 2
             *  [12]Número do Logradouro                            - 5
             *  [13]Complemento do Logradouro                       - 20
             *  [14]Valor Declarado                                 - 5
             *  [15]DDD + Telefone Destinatário                     - 12
             *  [16]Latitude                                        - 10
             *  [17]Longitude                                       - 10
             *  [18]Pipe “|”                                        - 1
             *  [19]Reserva para cliente                            - 30
             */

            string dataMatrix = string.Concat(new object[] { 
                /*[1]*/cepDestino,
                /*[2]*/NumberHelper.Formatar(number,5),
                /*[3]*/cepOrigem,
                /*[4]*/NumberHelper.Formatar(_correiosSettings.NumeroRemetenteSIGEP,5),
                /*[5]*/CorreiosHelper.ObterCodigoVerificadorCorreios(cepDestino),
                /*[6]*/"51",
                /*[7]*/plpShipment.Etiqueta.CodigoEtiquetaComVerificador,
                /*[8]*/NumberHelper.Formatar(CorreiosHelper.ObterServicosAdicionaisEtiqueta(_correiosSettings), 12, enumZeroComplete.Direita),
                /*[9]*/NumberHelper.ObterApenasNumeros(_correiosSettings.CartaoPostagemSIGEP),
                /*[10]*/plpShipment.Etiqueta.CodigoServico,
                /*[11]*/"00",
                /*[12]*/NumberHelper.Formatar(number,5),
                /*[13]*/complement,
                /*[14]*/NumberHelper.Formatar(NumberHelper.ObterApenasNumeros(plpShipment.ValorDeclarado.ToString("N0")), 5),
                /*[15]*/NumberHelper.Formatar(AddressHelper.FormatarCelular(shipment.Order.ShippingAddress.PhoneNumber), 12),
                /*[16]*/"-00.000000",
                /*[17]*/"-00.000000",
                /*[18]*/"|",
                /*[19]*/shipment.OrderId.ToString()});


            var arial9Normal = FontFactory.GetFont("Arial", 9, Font.NORMAL);
            var arial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            var arial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);

            var etiquetaTable = new PdfPTable(4);
            //A primeira é margem de 5 mm vazia

            var pointsValue = Utilities.MillimetersToPoints(106.36f);
            etiquetaTable.TotalWidth = pointsValue;

            /*
                Medidas	
                106,36 mm	100%
                5,36        5,04%
                31	 		29,15%
                44			41,37%
                26			24,45%	
            */

            //Quatro - 1 vazia Três colunas
            etiquetaTable.SetWidths(new[] { 0.05f, 0.29f, 0.41f, 0.24f });
            etiquetaTable.DefaultCell.Border = Rectangle.NO_BORDER;
            etiquetaTable.DefaultCell.BorderWidth = 0;


            var pointsValue25 = Utilities.MillimetersToPoints(26f);
            var pointsValue20 = Utilities.MillimetersToPoints(20f);


            string pathImages = ImageHelper.ObterPathImagens();

            var logo = Image.GetInstance(new Uri(Path.Combine(pathImages, "logo.png") ));
            logo.ScaleToFit(pointsValue25, pointsValue25);

            var dataMatrixImagem = ImageHelper.GetDataMatrix(dataMatrix);
            dataMatrixImagem.ScaleToFit(pointsValue25, pointsValue25);
            dataMatrixImagem.Alignment = Element.ALIGN_CENTER;

            string codigoServicoPedido = CorreiosServices.ObterCodigoEnvio(shipment.Order.ShippingMethod, _correiosSettings.CarrierServicesOffered);
            string servicoNomePublico = CorreiosServices.GetServicePublicNameById(codigoServicoPedido);
            
            var servico = Image.GetInstance(new Uri(Path.Combine(pathImages, string.Concat(servicoNomePublico, ".png"))));
            servico.ScaleToFit(pointsValue20, pointsValue20);


            var cellVazia = new PdfPCell();
            cellVazia.Border = Rectangle.NO_BORDER;

            var cellLogo = new PdfPCell();
            cellLogo.Border = Rectangle.NO_BORDER;
            cellLogo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellLogo.AddElement(logo);

            var cellDataMatrix = new PdfPCell();
            cellDataMatrix.Border = Rectangle.NO_BORDER;
            cellDataMatrix.HorizontalAlignment = Element.ALIGN_CENTER;
            cellDataMatrix.AddElement(dataMatrixImagem);

            var cellServico = new PdfPCell();
            cellServico.Border = Rectangle.NO_BORDER;
            cellServico.AddElement(servico);

            etiquetaTable.AddCell(cellVazia);
            etiquetaTable.AddCell(cellLogo);
            etiquetaTable.AddCell(cellDataMatrix);
            etiquetaTable.AddCell(cellServico);

            //Segunda Linha
            etiquetaTable.AddCell(cellVazia);

            var cellNotaFiscal = new PdfPCell(new Phrase("NF:", arial9Normal));
            cellNotaFiscal.Phrase.Add(new Phrase(shipment.OrderId.ToString(), arial9Normal));
            cellNotaFiscal.HorizontalAlignment = Element.ALIGN_LEFT;
            cellNotaFiscal.Border = Rectangle.NO_BORDER;
            etiquetaTable.AddCell(cellNotaFiscal);

            var cellContrato = new PdfPCell(new Phrase("Contrato:", arial9Normal));
            cellContrato.Phrase.Add(new Phrase(_correiosSettings.NumeroContratoSIGEP, arial9Bold));
            cellContrato.Border = Rectangle.NO_BORDER;
            cellContrato.HorizontalAlignment = Element.ALIGN_CENTER;
            etiquetaTable.AddCell(cellContrato);

            var cellVolume = new PdfPCell(new Phrase("Volume:", arial9Normal));
            cellVolume.Phrase.Add(new Phrase("1/1", arial9Normal));
            cellVolume.Border = Rectangle.NO_BORDER;
            etiquetaTable.AddCell(cellVolume);

            //Terceira Lina 
            etiquetaTable.AddCell(cellVazia);

            var cellPedido = new PdfPCell(new Phrase("Pedido:", arial9Normal));
            cellPedido.Phrase.Add(new Phrase(shipment.OrderId.ToString(), arial9Normal));
            cellPedido.HorizontalAlignment = Element.ALIGN_LEFT;
            cellPedido.Border = Rectangle.NO_BORDER;
            etiquetaTable.AddCell(cellPedido);

            var cellServicoDescricao = new PdfPCell(new Phrase(servicoNomePublico, arial9Bold));
            cellServicoDescricao.Border = Rectangle.NO_BORDER;
            cellServicoDescricao.HorizontalAlignment = Element.ALIGN_CENTER;
            etiquetaTable.AddCell(cellServicoDescricao);

            var cellPeso = new PdfPCell(new Phrase("Peso (g):", arial9Normal));

            string orderWeigth = string.Empty;
            if (_correiosSettings.UsarPesoPadraoSIGEP)
                orderWeigth = _correiosSettings.PesoPadraoSIGEP;
            else
                orderWeigth = plpShipment.PesoEstimado.ToString("N1");

            cellPeso.Phrase.Add(new Phrase(orderWeigth, arial9Bold));
            cellPeso.Border = Rectangle.NO_BORDER;
            etiquetaTable.AddCell(cellPeso);

            //Quarta linha

            etiquetaTable.AddCell(cellVazia);

            var cellCodigoEtiqueta = new PdfPCell(new Phrase(plpShipment.Etiqueta.CodigoEtiquetaComVerificador, arial11Bold));
            cellCodigoEtiqueta.Colspan = 3;
            cellCodigoEtiqueta.Border = Rectangle.NO_BORDER;
            cellCodigoEtiqueta.HorizontalAlignment = Element.ALIGN_CENTER;
            etiquetaTable.AddCell(cellCodigoEtiqueta);

            //Quinta linha

            etiquetaTable.AddCell(cellVazia);

            var pointsValue80 = Utilities.MillimetersToPoints(80f);
            var pointsValue18 = Utilities.MillimetersToPoints(18f);

            var codigoBarrasEtiqueta = ImageHelper.GetBarCode(shipment.TrackingNumber, pdfWriter);

            var cellCodigoBarrasEtiqueta = new PdfPCell();
            cellCodigoBarrasEtiqueta.Colspan = 3;
            cellCodigoBarrasEtiqueta.Border = Rectangle.NO_BORDER;
            cellCodigoBarrasEtiqueta.AddElement(codigoBarrasEtiqueta);

            etiquetaTable.AddCell(cellCodigoBarrasEtiqueta);

            //Sexta linha
            etiquetaTable.AddCell(cellVazia);

            var cellRecebedor = new PdfPCell(new Phrase("Recebedor:_____________________________________________", arial9Normal));
            cellRecebedor.Colspan = 3;
            cellRecebedor.Border = Rectangle.NO_BORDER;
            cellRecebedor.HorizontalAlignment = Element.ALIGN_LEFT;
            etiquetaTable.AddCell(cellRecebedor);

            //Setima linha
            etiquetaTable.AddCell(cellVazia);

            var cellAssinaturaDocumento = new PdfPCell(new Phrase("Assinatura:___________________Documento:_______________", arial9Normal));
            cellAssinaturaDocumento.Colspan = 3;
            cellAssinaturaDocumento.Border = Rectangle.NO_BORDER;
            cellAssinaturaDocumento.HorizontalAlignment = Element.ALIGN_LEFT;
            etiquetaTable.AddCell(cellAssinaturaDocumento);

            //Destinatário
            var cellDestinatario = new PdfPCell();
            cellDestinatario.Colspan = 4;
            cellDestinatario.BorderWidth = 0;
            cellDestinatario.Border = Rectangle.NO_BORDER;
            PdfPTable destinatario = ObterDestinatario(shipment.Order.ShippingAddress, pdfWriter);
            cellDestinatario.AddElement(destinatario);
            etiquetaTable.AddCell(cellDestinatario);


            //Remetente
            var cellRemetente = new PdfPCell();
            cellRemetente.Colspan = 4;
            cellRemetente.BorderWidth = 0;
            cellRemetente.Border = Rectangle.NO_BORDER;
            PdfPTable remetente = ObterRemetente(cidadeOrigem, cepOrigem, estadoOrigem);
            cellRemetente.AddElement(remetente);
            etiquetaTable.AddCell(cellRemetente);


            return etiquetaTable;
        }

        private PdfPTable ObterRemetente(string cidadeOrigem, string cepOrigem, string estadoOrigem)
        {
            var remetenteTable = new PdfPTable(2);
            remetenteTable.DefaultCell.Border = Rectangle.NO_BORDER;
            remetenteTable.WidthPercentage = 100;
            remetenteTable.DefaultCell.Border = 0;
            remetenteTable.DefaultCell.BorderWidth = 0;
            remetenteTable.DefaultCell.PaddingBottom = 0;
            remetenteTable.DefaultCell.PaddingTop = 0;
            remetenteTable.DefaultCell.VerticalAlignment = 0;

            /*	106,36	100%
                2,68	2,52%
                103,68	97,48%
            */

            var pointsValue = Utilities.MillimetersToPoints(106.36f);
            remetenteTable.TotalWidth = pointsValue;

            remetenteTable.SetWidths(new[] { 0.0252f, 0.9748f });

            var arial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            var arial10 = FontFactory.GetFont("Arial", 10, Font.NORMAL);

            var cellVazia = new PdfPCell();
            cellVazia.Border = Rectangle.NO_BORDER;

            remetenteTable.AddCell(cellVazia);


            
            var cellRemetenteNome = new PdfPCell(new Phrase("Remetente: ", arial10Bold));

            string remetente = StringHelper.Formatar(_correiosSettings.NomeRemetenteSIGEP, 50);
            cellRemetenteNome.Phrase.Add(new Phrase(remetente, arial10));
            cellRemetenteNome.Border = Rectangle.NO_BORDER;
            cellRemetenteNome.HorizontalAlignment = Element.ALIGN_LEFT;
            remetenteTable.AddCell(cellRemetenteNome);

            remetenteTable.AddCell(cellVazia);

            string enderecoNumero = StringHelper.Formatar(_correiosSettings.LogradouroRemetenteSIGEP, 50);
            enderecoNumero = string.Concat(enderecoNumero, ",", _correiosSettings.NumeroRemetenteSIGEP);

            var cellRuaNumero = new PdfPCell(new Phrase(enderecoNumero, arial10));
            cellRuaNumero.Border = Rectangle.NO_BORDER;
            cellRuaNumero.HorizontalAlignment = Element.ALIGN_LEFT;
            remetenteTable.AddCell(cellRuaNumero);


            remetenteTable.AddCell(cellVazia);

            string bairroRemetente = StringHelper.Formatar(_correiosSettings.BairroRemetenteSIGEP, 50);
            var cellComplementoBairro = new PdfPCell(new Phrase(bairroRemetente, arial10));
            cellComplementoBairro.Border = Rectangle.NO_BORDER;
            cellComplementoBairro.HorizontalAlignment = Element.ALIGN_LEFT;
            remetenteTable.AddCell(cellComplementoBairro);


            remetenteTable.AddCell(cellVazia);

            var cellCEPCidadeEstado = new PdfPCell(new Phrase(AddressHelper.FormatarCEPHiffen(cepOrigem), arial10Bold));

            string cidadeRemetente = StringHelper.Formatar(cidadeOrigem, 50);
            string estadoRemetente = StringHelper.Formatar(estadoOrigem, 2);
            
            cellCEPCidadeEstado.Phrase.Add(new Phrase(string.Concat(" ", cidadeRemetente, "-", estadoRemetente), arial10));
            cellCEPCidadeEstado.Border = Rectangle.NO_BORDER;
            cellCEPCidadeEstado.HorizontalAlignment = Element.ALIGN_LEFT;
            remetenteTable.AddCell(cellCEPCidadeEstado);

            return remetenteTable;

        }

        private PdfPTable ObterDestinatario(Address shippingAddress, PdfWriter pdfWriter)
        {
            var destinatarioTable = new PdfPTable(4);

            destinatarioTable.WidthPercentage = 100;

            destinatarioTable.DefaultCell.Border = Rectangle.NO_BORDER;
            destinatarioTable.DefaultCell.Border = 0;


            /*
            Medidas	
            106,36 mm	100%
            5,36	5,04%
            30		28,21%
            10		9,40%
            61		57,35%
            */

            //Quatro colunas - 1 vazia + Três colunas
            var pointsValue = Utilities.MillimetersToPoints(106.36f);
            destinatarioTable.TotalWidth = pointsValue;

            destinatarioTable.SetWidths(new[] { 0.05f, 0.28f, 0.09f, 0.58f });

            //Oitava Destinatario e Imagem Correios
            var arial10BoldWhite = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
            var arial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            var arial10 = FontFactory.GetFont("Arial", 10, Font.NORMAL);


            var cellDestinatario = new PdfPCell(new Phrase("DESTINATÁRIO", arial10BoldWhite));
            cellDestinatario.Colspan = 2;
            cellDestinatario.BackgroundColor = BaseColor.BLACK;
            cellDestinatario.Border = Rectangle.NO_BORDER;
            destinatarioTable.AddCell(cellDestinatario);


            var pointsValue20 = Utilities.MillimetersToPoints(20f);

            string pathImages = ImageHelper.ObterPathImagens();

            var logoCorreios = Image.GetInstance(Path.Combine(pathImages, "logo_correios.png"));
            logoCorreios.Alignment = Element.ALIGN_RIGHT;
            logoCorreios.ScaleAbsoluteWidth(pointsValue20);

            var cellLogoCorreios = new PdfPCell();
            cellLogoCorreios.Colspan = 2;
            cellLogoCorreios.Border = Rectangle.NO_BORDER;
            cellLogoCorreios.AddElement(logoCorreios);

            destinatarioTable.AddCell(cellLogoCorreios);

            var cellVazia = new PdfPCell();
            cellVazia.Border = Rectangle.NO_BORDER;

            destinatarioTable.AddCell(cellVazia);

            string nomeDestinatario = string.Format("{0} {1}", shippingAddress.FirstName, shippingAddress.LastName);
            nomeDestinatario = StringHelper.Formatar(nomeDestinatario, 50);

            var cellServicoDestinatario = new PdfPCell(new Phrase(nomeDestinatario, arial10));
            cellServicoDestinatario.Colspan = 3;
            cellServicoDestinatario.Border = Rectangle.NO_BORDER;
            cellServicoDestinatario.HorizontalAlignment = Element.ALIGN_LEFT;
            destinatarioTable.AddCell(cellServicoDestinatario);

            destinatarioTable.AddCell(cellVazia);


            var number = string.Empty;
            var complement = string.Empty;

            new AddressHelper(_addressAttributeParser, _workContext).GetCustomNumberAndComplement(shippingAddress.CustomAttributes, out number, out complement);

            string enderecoDestinatario = StringHelper.Formatar(shippingAddress.Address1, 50);
            enderecoDestinatario = string.Concat(enderecoDestinatario, ",", number);

            var cellEnderecoDestinatario = new PdfPCell(new Phrase(enderecoDestinatario, arial10));
            cellEnderecoDestinatario.Colspan = 3;
            cellEnderecoDestinatario.Border = Rectangle.NO_BORDER;
            cellEnderecoDestinatario.HorizontalAlignment = Element.ALIGN_LEFT;
            destinatarioTable.AddCell(cellEnderecoDestinatario);

            destinatarioTable.AddCell(cellVazia);

            complement = StringHelper.Formatar(complement, 30);
            string bairro = StringHelper.Formatar(shippingAddress.Address2, 50);
            string complementoBairro = string.Concat(complement, " ", bairro);

            var cellEnderecoComplementoBairro = new PdfPCell(new Phrase(complementoBairro, arial10));
            cellEnderecoComplementoBairro.Colspan = 3;
            cellEnderecoComplementoBairro.Border = Rectangle.NO_BORDER;
            cellEnderecoComplementoBairro.HorizontalAlignment = Element.ALIGN_LEFT;
            destinatarioTable.AddCell(cellEnderecoComplementoBairro);


            destinatarioTable.AddCell(cellVazia);

            string cepDestino = AddressHelper.FormatarCEPHiffen(shippingAddress.ZipPostalCode);

            var cellCEPCidade = new PdfPCell(new Phrase(cepDestino, arial10Bold));

            string cidade = StringHelper.Formatar(shippingAddress.City, 50);

            cellCEPCidade.Phrase.Add(new Phrase(string.Concat(" ", cidade, "/", shippingAddress.StateProvince.Abbreviation), arial10));
            cellCEPCidade.Colspan = 3;
            cellCEPCidade.Border = Rectangle.NO_BORDER;
            cellCEPCidade.HorizontalAlignment = Element.ALIGN_LEFT;
            destinatarioTable.AddCell(cellCEPCidade);


            destinatarioTable.AddCell(cellVazia);

            var pointsValue40 = Utilities.MillimetersToPoints(38f);
            var pointsValue16 = Utilities.MillimetersToPoints(14f);

            var codigoBarrasEndereco = ImageHelper.GetBarCode(NumberHelper.ObterApenasNumeros(cepDestino), pdfWriter);

            var cellCodigoBarrasEtiqueta = new PdfPCell();
            cellCodigoBarrasEtiqueta.Colspan = 2;
            cellCodigoBarrasEtiqueta.Border = Rectangle.NO_BORDER;
            cellCodigoBarrasEtiqueta.AddElement(codigoBarrasEndereco);
            destinatarioTable.AddCell(cellCodigoBarrasEtiqueta);

            destinatarioTable.AddCell(cellVazia);


            return destinatarioTable;
        }       
        
    }
}
