using Nop.Core.Domain.Customers;
using Nop.Plugin.Shipping.Correios.CalcPrecoPrazoWebReference;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nop.Plugin.Shipping.Correios
{
	public class CorreiosBatchCalculation
	{
		#region Fields
		public readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

		public string CodigoEmpresa { get; set; }
		public string Senha { get; set; }
		public string CepOrigem { get; set; }
		public string CepDestino { get; set; }
		public string Servicos { get; set; }
		public bool MaoPropria { get; set; }
		public bool AvisoRecebimento { get; set; }
		public List<Pacote> Pacotes {get; set;}

		private ILogger _logger;
        private Customer _customer;
        #endregion

        #region Ctor
        public CorreiosBatchCalculation(ILogger logger, Customer Customer)
		{
			Pacotes = new List<Pacote>();
			_logger = logger;
            _customer = Customer;

        }
		#endregion

		#region Methods
		/// <summary>
		/// Calculates the shipping costs with Correios web service.
		/// </summary>
		/// <returns>List of results returned from Correios web service.</returns>
		public cResultado Calculate()
		{
			int count = Pacotes.Count();

			if (count == 0)
			{
				return null;
			}
			else if (count == 1)
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = PtBrCulture;

				Pacote p = Pacotes[0];

				try
				{
					using (CalcPrecoPrazoWS client = new CalcPrecoPrazoWS())
					{
#if DEBUG
					//client.Proxy = new System.Net.WebProxy("127.0.0.1:8888");
#endif
						return client.CalcPrecoPrazo(
							CodigoEmpresa,
							Senha,
							Servicos,
							CepOrigem,
							CepDestino,
							p.Peso.ToString("0.00", PtBrCulture),
							(p.FormatoPacote ? 1 : 2),
							p.Comprimento,
							p.Altura,
							p.Largura,
							p.Diametro,
							(MaoPropria ? "S" : "N"),
							p.ValorDeclarado,
							(AvisoRecebimento ? "S" : "N"));
					}
				}
				catch (Exception ex)
				{
					_logger.Error("Plugin.Shipping.Correios: - Erro ao chamar WS dos Correios.\n" + ex.ToString(), ex, _customer);

					return null;
				}
			}
			else
			{
				//Para uma lógica futura de divisão de produtos em pacotes.
				return null;
			}
		}

		#endregion

		#region Pacote
		public class Pacote
		{
			public decimal Peso { get; set; }
			public bool FormatoPacote { get; set; }
			public decimal Comprimento { get; set; }
			public decimal Altura { get; set; }
			public decimal Largura { get; set; }
			public decimal Diametro { get; set; }
			public decimal ValorDeclarado { get; set; }
		}
		#endregion
	}
}
