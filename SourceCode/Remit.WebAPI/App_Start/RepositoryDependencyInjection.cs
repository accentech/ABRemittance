using Remit.Data.Repository;
using Unity;
using Unity.Lifetime;

namespace Remit.WebAPI
{
    public class RepositoryDependencyInjection
    {
        private readonly UnityContainer _container;
        public RepositoryDependencyInjection(UnityContainer container)
        {
            _container = container;
        }

        public void InjectAll()
        {
            _container.RegisterType<ICountryRepository, CountryRepository>(new HierarchicalLifetimeManager());
            _container.RegisterType<IBankRepository, BankRepository>(new HierarchicalLifetimeManager());
        }
    }
}