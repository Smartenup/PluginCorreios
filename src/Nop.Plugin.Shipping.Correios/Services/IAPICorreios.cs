using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Prazo;
using Nop.Plugin.Shipping.Correios.Domain.CorreiosAPI.Preco;
using Nop.Services.Shipping.Tracking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public interface IAPICorreios
    {
        void CheckShipmentDelived(Shipment shipment);
        Task<IList<ShipmentStatusEvent>> GetShipmentEventsAsync(string trackingNumber);

        Task<IList<PrazoResponse>> GetPrazoResponsesAsync(PrazosRequest prazoRequest);

        Task<IList<PrecoResponse>> GetPrecoResponsesAsync(PrecosRequest precoRequest);

    }
}
