using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using System.Reflection;
using Remit.Data.Repository;
using Remit.Data.Infrastructure;
using Remit.Service;
using Microsoft.AspNet.Identity.EntityFramework;
using Remit.Model.Models;
using Remit.Data.Models;
using Microsoft.AspNet.Identity;

namespace Remit.Web
{
    public static class Bootstrapper
    {
        public static void Run()
        {
            SetAutofacContainer();
            //Configure AutoMapper
            //AutoMapperConfiguration.Configure();
        }
        private static void SetAutofacContainer()
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
            builder.RegisterType<DatabaseFactory>().As<IDatabaseFactory>().InstancePerRequest();

            builder.RegisterAssemblyTypes(typeof(UserRepository).Assembly)
            .Where(t => t.Name.EndsWith("Repository"))
            .AsImplementedInterfaces().InstancePerRequest();

            builder.RegisterAssemblyTypes(typeof(UserService).Assembly)
           .Where(t => t.Name.EndsWith("Service"))
           .AsImplementedInterfaces().InstancePerRequest();

            //builder.RegisterAssemblyTypes(typeof(DefaultFormsAuthentication).Assembly)
            //.Where(t => t.Name.EndsWith("Authentication"))
            //.AsImplementedInterfaces().InstancePerHttpRequest();

            //builder.Register(c => new UserManager<ApplicationUser>(new UserStore<ApplicationUser>( new SampleEntities())))
            //.As<UserManager<ApplicationUser>>().InstancePerHttpRequest();

            builder.RegisterFilterProvider();
            IContainer container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));            
        }
    }
}