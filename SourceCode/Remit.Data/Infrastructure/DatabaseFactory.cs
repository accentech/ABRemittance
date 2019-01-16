using Remit.Data.Models;

namespace Remit.Data.Infrastructure
{
public class DatabaseFactory : Disposable, IDatabaseFactory
{
    private ApplicationEntities dataContext;
    public ApplicationEntities Get()
    {
        return dataContext ?? (dataContext = new ApplicationEntities());
    }
    protected override void DisposeCore()
    {
        if (dataContext != null)
            dataContext.Dispose();
    }
}
}
