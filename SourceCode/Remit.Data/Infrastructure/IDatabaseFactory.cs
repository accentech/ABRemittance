using System;
using Remit.Data.Models;

namespace Remit.Data.Infrastructure
{
    public interface IDatabaseFactory : IDisposable
    {
        ApplicationEntities Get();
    }
}
