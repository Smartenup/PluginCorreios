using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nop.Plugin.Shipping.Correios.Models
{
    public class PLPFechadaDetalheModel : BaseNopModel
    {
        public PLPFechadaDetalheModel()
        {
            Items = new List<PLPFechadaDetalheItemModel>();
        }

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

        public List<PLPFechadaDetalheItemModel> Items {get;set;}
    }
}
