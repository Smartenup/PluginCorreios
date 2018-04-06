using Nop.Services.Localization;

namespace Nop.Plugin.Shipping.Correios.Domain
{
    public class MensagemErroProcessamentoEtiqueta
    {
        private readonly ILocalizationService _localizationService;

        public MensagemErroProcessamentoEtiqueta(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public const int ETIQUETA_NAO_ENCONTRADA = 1;
        public const int CEP_INVALIDO = 2;
        public const int SERVICO_CORREIOS_INVALIDO_CEP = 3;


        /// <summary>
        /// <para>Obtem mensagem para o codigo de erro correspondente</para>
        /// <para>string Format Params: 0 - ShipmentMethod | 1 - ZipPostalCode | 2 - OrderId</para>
        /// </summary>
        /// <param name="codigoMensagem"> Codigo de erro processamento da etiqueta</param>
        /// <returns></returns>
        public string MensagemErro(int codigoMensagem)
        {
            string mensagem = string.Empty;
            
            switch (codigoMensagem)
            {
                case ETIQUETA_NAO_ENCONTRADA :
                    mensagem = _localizationService.GetResource("Plugins.Shippings.Correios.EtiquetaNaoEncontrada");
                    break;
                case CEP_INVALIDO:
                    mensagem = _localizationService.GetResource("Plugins.Shippings.Correios.CEPInvalido");
                    break;
                case SERVICO_CORREIOS_INVALIDO_CEP:
                    mensagem = _localizationService.GetResource("Plugins.Shippings.Correios.ServicoCorreiosInvalidoCEP");
                    break;
                default:
                    break;
            }

            return mensagem;
        }
    }

}
