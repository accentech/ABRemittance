using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Data.Infrastructure;
using Remit.Model.Models;

namespace Remit.Data.Repository
{
    public class SubModuleItemRepository: RepositoryBase<SubModuleItem>, ISubModuleItemRepository
    {
        public SubModuleItemRepository(IDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }

    public interface ISubModuleItemRepository : IRepository<SubModuleItem>
    {
    }
}
