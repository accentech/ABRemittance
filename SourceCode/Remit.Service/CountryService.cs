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
    public interface ICountryService
    {
        bool CreateCountry(Country country);
        bool UpdateCountry(Country country);
        bool DeleteCountry(string id);
        Country GetCountry(string id);
        
        IEnumerable<Country> GetAllCountry();
        void SaveRecord();

        bool CheckIsExist(Country country);
    }

    public class CountryService : ICountryService
    {
        private readonly ICountryRepository countryRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(CountryService));

        public CountryService()
        {
        }
                
        public CountryService(ICountryRepository countryRepository, IUnitOfWork unitOfWork)
        {
            this.countryRepository = countryRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CheckIsExist(Country country)
        {
            return countryRepository.Get(chk => chk.Name == country.Name) == null ? false : true;
        }

        public bool CreateCountry(Country country)
        {
            bool isSuccess = true;
            try
            {
                countryRepository.Add(country);                
                this.SaveRecord();
                ServiceUtil<Country>.WriteActionLog(country.Id, ENUMOperation.CREATE, country);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Country", ex);
            }
            return isSuccess;
        }

        public bool UpdateCountry(Country country)
        {
            bool isSuccess = true;
            try
            {
                countryRepository.Update(country);
                this.SaveRecord();
                ServiceUtil<Country>.WriteActionLog(country.Id, ENUMOperation.UPDATE, country);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Country", ex);
            }
            return isSuccess;
        }

        public bool DeleteCountry(string id)
        {
            bool isSuccess = true;
            var country = countryRepository.GetById(id);
            try
            {
                countryRepository.Delete(country);
                SaveRecord();
                ServiceUtil<Country>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Country", ex);
            }
            return isSuccess;
        }

        public Country GetCountry(string id)
        {
            return countryRepository.GetById(id);
        }
               
        public IEnumerable<Country> GetAllCountry()
        {
            return countryRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
