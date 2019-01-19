using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Data.Infrastructure;
using Remit.Model.Models;

namespace Remit.Data.Repository
{
    public class SubAgentRepository : RepositoryBase<SubAgent>, ISubAgentRepository
    {
        public SubAgentRepository(IDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }

    public interface ISubAgentRepository : IRepository<SubAgent>
    {
    }

}
