using Nop.Core.Data;
using Nop.Plugin.Shipping.Correios.Domain;
using System.Linq;

namespace Nop.Plugin.Shipping.Correios.Services
{
    internal class ValorDeclaradoService : IValorDeclaradoService
    {
        private readonly IRepository<ValorDeclarado> _valorDeclaradoRepository;
        public ValorDeclaradoService(IRepository<ValorDeclarado> valorDeclaradoRepository)
        {
            _valorDeclaradoRepository = valorDeclaradoRepository;
        }

        public ValorDeclarado ObterValorDeclarado(int servicoCorreiosId)
        {
            var query = from vlrDel in _valorDeclaradoRepository.Table
                        where vlrDel.ServicoCorreiosId == servicoCorreiosId
                        select vlrDel;

            return query.FirstOrDefault();

        }
    }
}
