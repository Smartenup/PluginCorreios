using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Prazo;
using Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Preco;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public interface IEmbalagemServicoCorreiosService
    {
        IList<EmbalagemServicoCorreios> ObterEmbalagemServicosCorreios(decimal pesoEnvio, decimal valorDeclarado);

        PrazosRequest GetPrazosRequest(IList<EmbalagemServicoCorreios> embalagemServicosCorreios, string zipPostalCodeReceiver, 
            string zipPostalCodeSender);

        PrecosRequest GetPrecosRequest(IList<EmbalagemServicoCorreios> embalagemServicosCorreios, string zipPostalCodeReceiver,
            string zipPostalCodeSender, int weight, int length, int height, int width, decimal baseValor);

    }
}