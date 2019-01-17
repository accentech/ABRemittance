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
    public interface ICityService
    {
        bool CreateCity(City city);
        bool UpdateCity(City city);
        bool DeleteCity(int id);
        City GetCity(int id);
        
        IEnumerable<City> GetAllCity();
        void SaveRecord();

        bool CheckIsExist(City city);
    }

    public class CityService : ICityService
    {
        private readonly ICityRepository cityRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(CityService));

        public CityService()
        {
        }
                
        public CityService(ICityRepository cityRepository, IUnitOfWork unitOfWork)
        {
            this.cityRepository = cityRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CheckIsExist(City city)
        {
            return cityRepository.Get(chk => chk.Name == city.Name) == null ? false : true;
        }

        public bool CreateCity(City city)
        {
            bool isSuccess = true;
            try
            {
                cityRepository.Add(city);                
                this.SaveRecord();
                ServiceUtil<City>.WriteActionLog(city.Id, ENUMOperation.CREATE, city);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating City", ex);
            }
            return isSuccess;
        }

        public bool UpdateCity(City city)
        {
            bool isSuccess = true;
            try
            {
                cityRepository.Update(city);
                this.SaveRecord();
                ServiceUtil<City>.WriteActionLog(city.Id, ENUMOperation.UPDATE, city);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating City", ex);
            }
            return isSuccess;
        }

        public bool DeleteCity(int id)
        {
            bool isSuccess = true;
            var city = cityRepository.GetById(id);
            try
            {
                cityRepository.Delete(city);
                SaveRecord();
                ServiceUtil<City>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting City", ex);
            }
            return isSuccess;
        }

        public City GetCity(int id)
        {
            return cityRepository.GetById(id);
        }
               
        public IEnumerable<City> GetAllCity()
        {
            return cityRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
