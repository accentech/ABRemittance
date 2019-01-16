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
    public interface IBankService
    {
        bool CreateBank(Bank bank);
        bool UpdateBank(Bank bank);
        
        bool DeleteBank(int id);
        Bank GetBank(int id);
        IEnumerable<Bank> GetAllBank();
        void SaveRecord();
       
        bool CheckIsExist(Bank bank);
        
    }

    public class BankService : IBankService
    {
        private readonly IBankRepository bankRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(BankService));

        public BankService()
        {
        }
        public BankService(IBankRepository bankRepository, IUnitOfWork unitOfWork)
        {

            this.bankRepository = bankRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(Bank bank)
        {

            return bankRepository.Get(chk => chk.Name == bank.Name) == null ? false : true;
        }

        public bool CreateBank(Bank bank)
        {
            bool isSuccess = true;
            try
            {
                bankRepository.Add(bank);
                this.SaveRecord();
                ServiceUtil<Bank>.WriteActionLog(bank.Id, ENUMOperation.CREATE, bank);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Bank", ex);
            }
            return isSuccess;
        }

        public bool UpdateBank(Bank bank)
        {
            bool isSuccess = true;
            try
            {
                bankRepository.Update(bank);
                this.SaveRecord();
                ServiceUtil<Bank>.WriteActionLog(bank.Id, ENUMOperation.UPDATE, bank);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Bank", ex);
            }
            return isSuccess;
        }

        public bool DeleteBank(int id)
        {
            bool isSuccess = true;
            var bank = bankRepository.GetById(id);
            try
            {
                bankRepository.Delete(bank);
                SaveRecord();
                ServiceUtil<Bank>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Bank", ex);
            }
            return isSuccess;
        }

        public Bank GetBank(int id)
        {
            return bankRepository.GetById(id);
        }

        public IEnumerable<Bank> GetAllBank()
        {
            return bankRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
