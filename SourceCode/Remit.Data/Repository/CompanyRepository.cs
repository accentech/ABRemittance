using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Data.Infrastructure;
using Remit.Model.Models;

namespace Remit.Data.Repository
{
    public class CompanyRepository: RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(IDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }

    public interface ICompanyRepository : IRepository<Company>
    {
    }
}
