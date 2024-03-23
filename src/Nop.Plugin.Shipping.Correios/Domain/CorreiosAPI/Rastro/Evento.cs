using System;

namespace Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Rastro
{
    public class Evento
    {
        public string Codigo { get; set; }
        public string Tipo { get; set; }
        public DateTime DtHrCriado { get; set; }
        public string Descricao { get; set; }
        public Unidade Unidade { get; set; }
        public string Detalhe { get; set; }
        public string Comentario { get; set; }
        public EntregadorExterno EntregadorExterno { get; set; }
        public UnidadeDestino UnidadeDestino { get; set; }
        public DateTime? DtLimiteRetirada { get; set; }
    }
}
