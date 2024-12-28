using Nop.Plugin.Shipping.Correios.Domain;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public interface IValorDeclaradoService
    {
        ValorDeclarado ObterValorDeclarado(int servicoCorreiosId);
    }
}
