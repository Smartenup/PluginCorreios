using System.Text.RegularExpressions;

namespace Nop.Plugin.Shipping.Correios.Util
{
    public enum enumZeroComplete
    {
        Esquerda,
        Direita,
        Nenhum
    }


    public static class NumberHelper
    {
        public static string ObterApenasNumeros(string stringValue)
        {
            var r = new Regex(@"\d+");

            var result = string.Empty;
            foreach (Match m in r.Matches(stringValue))
                result += m.Value;

            return result;
        }

        public static string ObterApenasNumerosInteiro(string stringValue)
        {
            int posicaoPonto = stringValue.LastIndexOf(".");

            var result = string.Empty;

            if (posicaoPonto > -1)
                result = stringValue.Substring(0, posicaoPonto);
            else
                result = stringValue;

            result = ObterApenasNumeros(result);

            return result; ;
        }

        public static string Formatar(string stringValue, int tamanho, enumZeroComplete zeroComplete = enumZeroComplete.Esquerda)
        {
            string retorno = string.Empty;

            if (stringValue.Length > tamanho)
            { 
                retorno = stringValue.Substring(0, tamanho);
            }
            else
            {
                retorno = stringValue;

                switch (zeroComplete)
                {
                    case enumZeroComplete.Esquerda:
                        while (retorno.Length != tamanho)
                            retorno = "0" + retorno;
                        break;
                    case enumZeroComplete.Direita:
                        while (retorno.Length != tamanho)
                            retorno += "0";

                        break;
                    default:
                        break;
                }
            }

            return retorno;

        }
    }
}
