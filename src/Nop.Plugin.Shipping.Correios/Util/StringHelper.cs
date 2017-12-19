namespace Nop.Plugin.Shipping.Correios.Util
{
    public class StringHelper
    {

        /// <summary>
        /// Limita ao tamanho máximo (trunc) e coloca tudo em caixa alta (UPPERCASE)
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="tamanho"></param>
        /// <param name="notUpperCaseAll"></param>
        /// <returns></returns>
        public static string Formatar(string valor, int tamanho, bool notUpperCaseAll = false)
        {
            string resultado = string.Empty;

            if (valor.Length > tamanho)
                resultado = valor.Substring(0, tamanho);
            else
                resultado = valor;

            if (!notUpperCaseAll)
                resultado = resultado.ToUpperInvariant();

            return resultado;
        }
    }
}
