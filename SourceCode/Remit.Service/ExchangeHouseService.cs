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
    public interface IExchangeHouseService
    {
        bool CreateExchangeHouse(ExchangeHouse exchangeHouse);
        bool UpdateExchangeHouse(ExchangeHouse exchangeHouse);
        
        bool DeleteExchangeHouse(int id);
        ExchangeHouse GetExchangeHouse(int id);
        IEnumerable<ExchangeHouse> GetAllExchangeHouse();
        void SaveRecord();
       
        bool CheckIsExist(ExchangeHouse exchangeHouse);
        
    }

    public class ExchangeHouseService : IExchangeHouseService
    {
        private readonly IExchangeHouseRepository exchangeHouseRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(ExchangeHouseService));

        public ExchangeHouseService()
        {
        }
        public ExchangeHouseService(IExchangeHouseRepository exchangeHouseRepository, IUnitOfWork unitOfWork)
        {

            this.exchangeHouseRepository = exchangeHouseRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(ExchangeHouse exchangeHouse)
        {

            return exchangeHouseRepository.Get(chk => chk.Name == exchangeHouse.Name) == null ? false : true;
        }

        public bool CreateExchangeHouse(ExchangeHouse exchangeHouse)
        {
            bool isSuccess = true;
            try
            {
                exchangeHouseRepository.Add(exchangeHouse);
                this.SaveRecord();
                ServiceUtil<ExchangeHouse>.WriteActionLog(exchangeHouse.Id, ENUMOperation.CREATE, exchangeHouse);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating ExchangeHouse", ex);
            }
            return isSuccess;
        }

        public bool UpdateExchangeHouse(ExchangeHouse exchangeHouse)
        {
            bool isSuccess = true;
            try
            {
                exchangeHouseRepository.Update(exchangeHouse);
                this.SaveRecord();
                ServiceUtil<ExchangeHouse>.WriteActionLog(exchangeHouse.Id, ENUMOperation.UPDATE, exchangeHouse);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating ExchangeHouse", ex);
            }
            return isSuccess;
        }

        public bool DeleteExchangeHouse(int id)
        {
            bool isSuccess = true;
            var exchangeHouse = exchangeHouseRepository.GetById(id);
            try
            {
                exchangeHouseRepository.Delete(exchangeHouse);
                SaveRecord();
                ServiceUtil<ExchangeHouse>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting ExchangeHouse", ex);
            }
            return isSuccess;
        }

        public ExchangeHouse GetExchangeHouse(int id)
        {
            return exchangeHouseRepository.GetById(id);
        }

        public IEnumerable<ExchangeHouse> GetAllExchangeHouse()
        {
            return exchangeHouseRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
