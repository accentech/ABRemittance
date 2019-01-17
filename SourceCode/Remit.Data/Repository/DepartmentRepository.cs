using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Data.Infrastructure;
using Remit.Model.Models;

namespace Remit.Data.Repository
{
    public class DepartmentRepository: RepositoryBase<Department>, IDepartmentRepository
    {
        public DepartmentRepository(IDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }

    public interface IDepartmentRepository : IRepository<Department>
    {
    }
}
