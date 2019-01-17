using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Data.Infrastructure;
using Remit.Model.Models;

namespace Remit.Data.Repository
{
    public class SubModuleRepository: RepositoryBase<SubModule>, ISubModuleRepository
    {
        public SubModuleRepository(IDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }

    public interface ISubModuleRepository : IRepository<SubModule>
    {
    }
}
