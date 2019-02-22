using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Shipping.Correios
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Shipping.Correios.GetAddresByCEP",
                            "Plugins/ShippingCorreios/GetAddresByCEP",
                            new { controller = "ShippingCorreios", action = "GetAddresByCEP" },
                            new[] { "Nop.Plugin.Shipping.Correios.Controllers" });

            routes.MapRoute("Plugin.Shipping.Correios.Configure",
                 "Plugins/ShippingCorreios/Configure",
                 new { controller = "ShippingCorreios", action = "Configure" },
                 new[] { "Nop.Plugin.Shipping.Correios.Controllers" });

            routes.MapRoute("Plugin.Shipping.Correios.PdfEtiquetaSelected",
                            "admin/Plugins/Shipping/ShippingCorreios/PdfEtiquetaSelected",
                            new { controller = "ShippingCorreiosPluginAdmin", action = "PdfEtiquetaSelected" },
                            new[] { "Nop.Plugin.Shipping.Correios.Controllers.Admin" });

            routes.MapRoute("Plugin.Shipping.Correios.PdfEtiquetaAll",
                            "admin/Plugins/Shipping/ShippingCorreios/PdfEtiquetaAll",
                            new { controller = "ShippingCorreiosPluginAdmin", action = "PdfEtiquetaAll" },
                            new[] { "Nop.Plugin.Shipping.Correios.Controllers.Admin" });

            routes.MapRoute("Plugin.Shipping.Correios.PLPAbertaItemDelete",
                            "admin/Plugins/Shipping/ShippingCorreios/PLPAbertaItemDelete",
                            new { controller = "ShippingCorreiosPluginAdmin", action = "PLPAbertaItemDelete" },
                            new[] { "Nop.Plugin.Shipping.Correios.Controllers.Admin" });

            routes.MapRoute("Plugin.Shipping.Correios.PLPAberta",
                            "admin/Plugins/Shipping/ShippingCorreios/PLPAberta",
                            new { controller = "ShippingCorreiosPluginAdmin", action = "PLPAberta" },
                            new[] { "Nop.Plugin.Shipping.Correios.Controllers.Admin" });

            routes.MapRoute("Plugin.Shipping.Correios.PLPAbertaListSelect",
                            "admin/Plugins/Shipping/ShippingCorreios/PLPAbertaListSelect",
                            new { controller = "ShippingCorreiosPluginAdmin", action = "PLPAbertaListSelect" },
                            new[] { "Nop.Plugin.Shipping.Correios.Controllers.Admin" });


            routes.MapRoute("Plugin.Shipping.Correios.PLPFechadaList",
                            "admin/Plugins/Shipping/ShippingCorreios/PLPFechadaList",
                            new { controller = "ShippingCorreiosPluginAdmin", action = "PLPFechadaList" },
                            new[] { "Nop.Plugin.Shipping.Correios.Controllers.Admin" });


            routes.MapRoute("Plugin.Shipping.Correios.PLPFechada",
                            "admin/Plugins/Shipping/ShippingCorreios/PLPFechada",
                            new { controller = "ShippingCorreiosPluginAdmin", action = "PLPFechada" },
                            new[] { "Nop.Plugin.Shipping.Correios.Controllers.Admin" });

            routes.MapRoute("Plugin.Shipping.Correios.PdfFechamento",
                            "admin/Plugins/Shipping/ShippingCorreios/PdfFechamento",
                            new { controller = "ShippingCorreiosPluginAdmin", action = "PdfFechamento" },
                            new[] { "Nop.Plugin.Shipping.Correios.Controllers.Admin" });

            routes.MapRoute("Plugin.Shipping.Correios.PLPFechadaDetalhe",
                            "admin/Plugins/Shipping/ShippingCorreios/PLPFechadaDetalhe",
                            new { controller = "ShippingCorreiosPluginAdmin", action = "PLPFechadaDetalhe" },
                            new[] { "Nop.Plugin.Shipping.Correios.Controllers.Admin" });

            routes.MapRoute("Plugin.Shipping.Correios.PLPFechadaDetalheSelect",
                            "admin/Plugins/Shipping/ShippingCorreios/PLPFechadaDetalheSelect",
                            new { controller = "ShippingCorreiosPluginAdmin", action = "PLPFechadaDetalheSelect" },
                            new[] { "Nop.Plugin.Shipping.Correios.Controllers.Admin" });

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
