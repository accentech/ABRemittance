using Remit.Data.Repository;
using Remit.Data.Infrastructure;
using Remit.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Core.Common;
using Remit.LoggerService;

namespace Remit.Service
{
    public interface ICompanyService
    {

        bool CreateCompany(Company company);
        bool UpdateCompany(Company company);
        bool DeleteCompany(int id);
        Company GetCompany(int id);
        
        IEnumerable<Company> GetAllCompany();
        void SaveRecord();

        bool CheckIsExist(Company company);
      Company GetCompanyByName(string name);
    }

    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository companyRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(CompanyService));

        public CompanyService()
        {
        }
                
        public CompanyService(ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
        {
            this.companyRepository = companyRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CheckIsExist(Company company)
        {
            return companyRepository.Get(chk => chk.Name == company.Name) == null ? false : true;
        }

        public bool CreateCompany(Company company)
        {
            bool isSuccess = true;
            try
            {
                companyRepository.Add(company);                
                this.SaveRecord();
                ServiceUtil<Company>.WriteActionLog(company.Id, ENUMOperation.CREATE, company);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Company", ex);
            }
            return isSuccess;
        }

        public bool UpdateCompany(Company company)
        {
            bool isSuccess = true;
            try
            {
                companyRepository.Update(company);
                this.SaveRecord();
                ServiceUtil<Company>.WriteActionLog(company.Id, ENUMOperation.UPDATE, company);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Company", ex);
            }
            return isSuccess;
        }

        public bool DeleteCompany(int id)
        {
            bool isSuccess = true;
            var company = companyRepository.GetById(id);
            try
            {
                companyRepository.Delete(company);
                SaveRecord();
                ServiceUtil<Company>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Company", ex);
            }
            return isSuccess;
        }

        public Company GetCompany(int id)
        {
            return companyRepository.GetById(id);
        }

        public Company GetCompanyByName( string name )
        {
          return companyRepository.Get(comp => comp.Name == name);
        }
               
        public IEnumerable<Company> GetAllCompany()
        {
            return companyRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
