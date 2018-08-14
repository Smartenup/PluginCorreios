using Nop.Web.Framework.Mvc;
using System;
using System.ComponentModel;

namespace Nop.Plugin.Shipping.Correios.Models
{
    public class PLPFechadaResultModel : BaseNopModel
    {
        [DisplayName("PLP ID Nop")]
        public int Id { get; set; }

        [DisplayName("PLP ID Correios")]
        public long CorreiosId { get; set; }

        [DisplayName("Data Fechamento")]
        public DateTime DataFechamento { get; set; }

        [DisplayName("Quantidade Objetos")]
        public int QuantidadeObjetos { get; set; }

        [DisplayName("Usuário Fechamento")]
        public string UsuarioFechamento { get; set; }
    }
}
