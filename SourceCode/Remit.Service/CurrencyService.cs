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
    public interface ICurrencyService
    {

        bool CreateCurrency(Currency currency);
        bool UpdateCurrency(Currency currency);
        bool DeleteCurrency(int id);
        Currency GetCurrency(int id);
        
        IEnumerable<Currency> GetAllCurrency();
        void SaveRecord();

        bool CheckIsExist(Currency currency);
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository currencyRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(CurrencyService));

        public CurrencyService()
        {
        }
                
        public CurrencyService(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork)
        {
            this.currencyRepository = currencyRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CheckIsExist(Currency currency)
        {
            return false;
            //return currencyRepository.Get(chk => chk.Name == currency.Name) == null ? false : true;
        }

        public bool CreateCurrency(Currency currency)
        {
            bool isSuccess = true;
            try
            {
                currencyRepository.Add(currency);                
                this.SaveRecord();
                ServiceUtil<Currency>.WriteActionLog(currency.Id, ENUMOperation.CREATE, currency);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Currency", ex);
            }
            return isSuccess;
        }

        public bool UpdateCurrency(Currency currency)
        {
            bool isSuccess = true;
            try
            {
                currencyRepository.Update(currency);
                this.SaveRecord();
                ServiceUtil<Currency>.WriteActionLog(currency.Id, ENUMOperation.UPDATE, currency);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Currency", ex);
            }
            return isSuccess;
        }

        public bool DeleteCurrency(int id)
        {
            bool isSuccess = true;
            var currency = currencyRepository.GetById(id);
            try
            {
                currencyRepository.Delete(currency);
                SaveRecord();
                ServiceUtil<Currency>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Currency", ex);
            }
            return isSuccess;
        }

        public Currency GetCurrency(int id)
        {
            return currencyRepository.GetById(id);
        }
               
        public IEnumerable<Currency> GetAllCurrency()
        {
            return currencyRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
