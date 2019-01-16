using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Data.Infrastructure;
using Remit.Model.Models;

namespace Remit.Data.Repository
{
    public class WorkflowactionRepository : RepositoryBase<Workflowaction>, IWorkflowactionRepository
    {
        public WorkflowactionRepository(IDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }

    public interface IWorkflowactionRepository : IRepository<Workflowaction>
    {
    }
}
