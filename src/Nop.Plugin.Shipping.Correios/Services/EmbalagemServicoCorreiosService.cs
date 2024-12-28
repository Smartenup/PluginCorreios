using Newtonsoft.Json.Linq;
using Nop.Core.Data;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Prazo;
using Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Preco;
using SmartenUP.Core.Services.Shippping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Shipping.Correios.Services
{
    internal class EmbalagemServicoCorreiosService : IEmbalagemServicoCorreiosService
    {
        private readonly IRepository<EmbalagemServicoCorreios> _embalagemServicoCorreiosRepository;
        private readonly IValorDeclaradoService _valorDeclaradoService;
        private readonly CorreiosSettings _correiosSettings;

        public EmbalagemServicoCorreiosService(IRepository<ServicoCorreios> servicoCorreioRepository,
            IRepository<Embalagem> embalagemRepository, 
            IRepository<ValorDeclarado> valorDeclaradoRepository,
            IRepository<EmbalagemServicoCorreios> embalagemServicoCorreiosRepository,
            CorreiosSettings correiosSettings,
            IValorDeclaradoService valorDeclaradoService)
        {
            _embalagemServicoCorreiosRepository = embalagemServicoCorreiosRepository;
            _correiosSettings = correiosSettings;
            _valorDeclaradoService = valorDeclaradoService;
        }

        public IList<EmbalagemServicoCorreios> ObterEmbalagemServicosCorreios(decimal pesoEnvio, decimal valorDeclarado)
        {
            var query = from esc in _embalagemServicoCorreiosRepository.Table
                        where pesoEnvio <= esc.PesoMaximo && esc.EmbalagemId == 2                         
                        select esc;

            return query.ToList();
        }

        public PrazosRequest GetPrazosRequest(IList<EmbalagemServicoCorreios> embalagemServicoCorreios, 
            string zipPostalCodeReceiver, string zipPostalCodeSender)
        {
            var prazoRequest = new PrazosRequest();

            prazoRequest.idLote = "001";

            var randomNumber = new Random();

            prazoRequest.parametrosPrazo = new List<ParametrosPrazo>();

            foreach (var embalagemServico in embalagemServicoCorreios)
            {
                var paramentro = new ParametrosPrazo();

                paramentro.cepDestino = zipPostalCodeReceiver;
                paramentro.cepOrigem = zipPostalCodeSender;
                paramentro.nuRequisicao = randomNumber.Next(0, int.MaxValue).ToString();
                paramentro.coProduto = embalagemServico.ServicoCorreios.CodigoServico;
                paramentro.dtEvento = DateTime.Now.ToString("dd/MM/yyyy");
                paramentro.dataPostagem = DateTime.Now.ToString("dd/MM/yyyy");

                prazoRequest.parametrosPrazo.Add(paramentro);
            }

            return prazoRequest;

        }



        public PrecosRequest GetPrecosRequest(IList<EmbalagemServicoCorreios> embalagemServicoCorreios, string zipPostalCodeReceiver, 
            string zipPostalCodeSender, int weight, int length, int height, int width, decimal baseValor) 
        
        { 
            var precoRequest = new PrecosRequest();

            precoRequest.IdLote = "001";
            precoRequest.ParametrosProduto = new List<ParametrosProduto>();

            var randomNumber = new Random();

            foreach (var embalagemServico in embalagemServicoCorreios)
            {
                var parametroProduto = new ParametrosProduto();

                parametroProduto.CoProduto = embalagemServico.ServicoCorreios.CodigoServico;
                parametroProduto.NuRequisicao = randomNumber.Next(0, int.MaxValue).ToString();
                parametroProduto.CepDestino = zipPostalCodeReceiver;
                parametroProduto.PsObjeto =  weight.ToString();
                parametroProduto.TpObjeto = "2"; //Verificar o que seria os codigo do tipo objeto ( caixa, envelope, cilindro e bla bla bla)

                int lengthRequest = length;
                if (embalagemServico.ComprimentoMinimo.HasValue && lengthRequest < embalagemServico.ComprimentoMinimo) 
                    lengthRequest = int.Parse(embalagemServico.ComprimentoMinimo.Value.ToString("N0"));
                lengthRequest = lengthRequest == 0 ? 1 : lengthRequest;
                parametroProduto.Comprimento = lengthRequest.ToString();

                int widthRequest = width;
                if (embalagemServico.LarguraMinimo.HasValue && widthRequest < embalagemServico.LarguraMinimo)
                    widthRequest = int.Parse(embalagemServico.LarguraMinimo.Value.ToString("N0"));
                widthRequest = widthRequest == 0 ? 1 : widthRequest;
                parametroProduto.Largura = widthRequest.ToString();

                int heightRequest = height;
                if (embalagemServico.AlturaMinimo.HasValue && widthRequest < embalagemServico.AlturaMinimo)
                    heightRequest = int.Parse(embalagemServico.AlturaMinimo.Value.ToString("N0"));
                heightRequest = heightRequest == 0 ? 1 : heightRequest;

                parametroProduto.Altura = heightRequest.ToString();
                parametroProduto.DtEvento = DateTime.Now.ToString("dd/MM/yyyy");

                parametroProduto.ServicosAdicionais = new List<ServicosAdicionalRequest>();

                var postagemServicosAdicionalRequest = new ServicosAdicionalRequest();
                postagemServicosAdicionalRequest.CoServAdicional = "001";
                parametroProduto.ServicosAdicionais.Add(postagemServicosAdicionalRequest);

                var valorDeclarado = _valorDeclaradoService.ObterValorDeclarado(embalagemServico.ServicoCorreios.Id);

                if (_correiosSettings.IncluirValorDeclarado && valorDeclarado != null && valorDeclarado.Id > 0)
                {
                    var valorDeclaradoServicosAdicionalRequest = new ServicosAdicionalRequest();
                    valorDeclaradoServicosAdicionalRequest.CoServAdicional = valorDeclarado.CodigoCorreiosValorDeclarado;
                    parametroProduto.ServicosAdicionais.Add(valorDeclaradoServicosAdicionalRequest);

                    decimal valorDeclaradoRequest = baseValor;

                    if (valorDeclaradoRequest < valorDeclarado.MinimoValorDeclarado)
                        valorDeclaradoRequest = valorDeclarado.MinimoValorDeclarado;

                    parametroProduto.VlDeclarado = valorDeclaradoRequest.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                }


                parametroProduto.CepOrigem = zipPostalCodeSender;

                precoRequest.ParametrosProduto.Add(parametroProduto);
            }

            return precoRequest;

        }
    }
}
