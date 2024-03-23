using System;

namespace Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Token
{
    public class Token
    {
        public string Ambiente { get; set; }
        public string Id { get; set; }
        public string Ip { get; set; }
        public string Perfil { get; set; }
        public string Cnpj { get; set; }
        public Contrato Contrato { get; set; }
        public DateTime Emissao { get; set; }
        public DateTime ExpiraEm { get; set; }
        public string ZoneOffset { get; set; }
        public string token { get; set; }

    }
}
