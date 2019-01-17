using Remit.Data.Infrastructure;
using Remit.Service;
using Unity;
using Unity.Lifetime;

namespace Remit.WebAPI
{
    public class ServiceDependencyInjection
    {
        private readonly UnityContainer _container;

        public ServiceDependencyInjection(UnityContainer container)
        {
            _container = container;
        }

        public void InjectAll()
        {
            _container.RegisterType<IUnitOfWork, UnitOfWork>(new HierarchicalLifetimeManager());
            _container.RegisterType<IDatabaseFactory, DatabaseFactory>(new HierarchicalLifetimeManager());
            _container.RegisterType<ICountryService, CountryService>(new HierarchicalLifetimeManager());
            _container.RegisterType<IBankService, BankService>(new HierarchicalLifetimeManager());
        }
    }
}