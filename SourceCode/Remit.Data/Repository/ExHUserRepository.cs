using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Data.Infrastructure;
using Remit.Model.Models;

namespace Remit.Data.Repository
{
    public class ExHUserRepository : RepositoryBase<ExHUser>, IExHUserRepository
    {
        public ExHUserRepository(IDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }

    public interface IExHUserRepository : IRepository<ExHUser>
    {
    }

}
