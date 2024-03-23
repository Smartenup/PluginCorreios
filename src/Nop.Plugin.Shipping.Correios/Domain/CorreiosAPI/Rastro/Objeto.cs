using System;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Rastro
{
    public class Objeto
    {
        public string CodObjeto { get; set; }
        public TipoPostal TipoPostal { get; set; }
        public DateTime DtPrevista { get; set; }
        public string Contrato { get; set; }
        public int Largura { get; set; }
        public int Comprimento { get; set; }
        public int Altura { get; set; }
        public double Peso { get; set; }
        public string Formato { get; set; }
        public string Modalidade { get; set; }
        public double ValorDeclarado { get; set; }
        public List<Evento> Eventos { get; set; }
    }
}
