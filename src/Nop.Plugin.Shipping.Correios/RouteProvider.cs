using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Shipping.Correios
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Shipping.Correios.Configure",
                 "Plugins/ShippingCorreios/Configure",
                 new { controller = "ShippingCorreios", action = "Configure" },
                 new[] { "Nop.Plugin.Shipping.Correios.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
