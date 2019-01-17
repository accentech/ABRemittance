using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Data.Infrastructure;
using Remit.Model.Models;

namespace Remit.Data.Repository
{
    public class ActionLogRepository: RepositoryBase<ActionLog>, IActionLogRepository
    {
        public ActionLogRepository(IDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }

    public interface IActionLogRepository : IRepository<ActionLog>
    {
    }
}
