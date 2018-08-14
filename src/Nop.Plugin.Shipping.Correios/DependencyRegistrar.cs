using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Shipping.Correios.Data;
using Nop.Plugin.Shipping.Correios.Domain;
using Nop.Plugin.Shipping.Correios.Services;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.Correios
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<SigepWebService>().As<ISigepWebService>().InstancePerLifetimeScope();
            builder.RegisterType<PdfSigepWebService>().As<IPdfSigepWebService>().InstancePerLifetimeScope();
            builder.RegisterType<SigepWebPlpService>().As<ISigepWebPlpService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<CorreiosObjectContext>(builder, "nop_object_context_correios");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<PlpSigepWeb>>()
                .As<IRepository<PlpSigepWeb>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_correios"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PlpSigepWebEtiqueta>>()
                .As<IRepository<PlpSigepWebEtiqueta>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_correios"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PlpSigepWebShipment>>()
                .As<IRepository<PlpSigepWebShipment>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_correios"))
                .InstancePerLifetimeScope();            
        }
    }
}
