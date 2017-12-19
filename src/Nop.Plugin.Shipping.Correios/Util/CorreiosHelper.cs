using System;

namespace Nop.Plugin.Shipping.Correios.Util
{
    public class CorreiosHelper
    {
        public static string ObterCodigoVerificadorCorreios(string cepDestino)
        {
            if (string.IsNullOrWhiteSpace(cepDestino))
                throw new ArgumentNullException("cepDestino");

            int somaCEP = 0;

            for (int i = 0; i < cepDestino.Length; i++)
                somaCEP += int.Parse(cepDestino[i].ToString());


            double divisor = 0.0;

            divisor = ((double)somaCEP / (double)10);

            if (divisor > (int)divisor)
                divisor = (int)divisor + 1;

            int resultado = (int)(divisor * 10) - somaCEP;

            return resultado.ToString();
        }


        public static string CalculaNumeroEtiquetaComVerificador(string NumeroEtiquetaCorreios)
        {

            int[] multiplicadores = { 8, 6, 4, 2, 3, 5, 9, 7 };
            int soma = 0;
            String dv;

            if (NumeroEtiquetaCorreios.Length != 8)
            {
                NumeroEtiquetaCorreios = "Error";
            }
            else
            {

                for (int i = 0; i < 8; i++)
                {
                    soma += int.Parse(NumeroEtiquetaCorreios.Substring(i, 1)) * multiplicadores[i];
                }

                int resto = soma % 11;

                if (resto == 0)
                {
                    dv = "5";
                }
                else if (resto == 1)
                {
                    dv = "0";
                }
                else
                {
                    dv = (11 - resto).ToString();
                }

                NumeroEtiquetaCorreios += dv;
            }

            return NumeroEtiquetaCorreios;

        }

       

        public static string ObterServicosAdicionaisEtiqueta(CorreiosSettings correiosSettings)
        {
            /*
             * Serviço Adicional Descrição
             * 01 - Aviso de Recebimento
             * 02 - Mão Própria Nacional
             * 19 - Valor Declarado Nacional (Encomendas)
             * 25 - Registro Nacional
             * 37 - Aviso de Recebimento Digital
             * 49 - Devolução de Nota Fiscal - SEDEX
             * 57 - Taxa de Entrega de Encomenda Despadronizada
             * 67 - Logística Reversa Simultânea Domiciliária
             * 69 - Logística Reversa Simultânea em Agência
             */

            string retorno = string.Empty;

            //O valor de registro nacional sempre deve ser enviado
            retorno = "25";

            string codigoAvisoRecebimento = "01";
            string codigoMaoPropria = "02";
            string codigoValorDeclarado = "19";

            if (correiosSettings.IncluirAvisoRecebimento)
                retorno += codigoAvisoRecebimento;

            if (correiosSettings.IncluirMaoPropria)
                retorno += codigoMaoPropria;

            if (correiosSettings.IncluirValorDeclarado)
                retorno += codigoValorDeclarado;

            return retorno;
        }
    }
}
