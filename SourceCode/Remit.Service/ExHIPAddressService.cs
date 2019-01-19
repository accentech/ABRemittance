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
    public interface IExHIPAddressService
    {
        bool CreateExHIPAddress(ExHIPAddress exHIPAddress);
        bool UpdateExHIPAddress(ExHIPAddress exHIPAddress);
        
        bool DeleteExHIPAddress(int id);
        ExHIPAddress GetExHIPAddress(int id);
        IEnumerable<ExHIPAddress> GetAllExHIPAddress();
        void SaveRecord();
       
        bool CheckIsExist(ExHIPAddress exHIPAddress);
        
    }

    public class ExHIPAddressService : IExHIPAddressService
    {
        private readonly IExHIPAddressRepository exHIPAddressRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(ExHIPAddressService));

        public ExHIPAddressService()
        {
        }
        public ExHIPAddressService(IExHIPAddressRepository exHIPAddressRepository, IUnitOfWork unitOfWork)
        {

            this.exHIPAddressRepository = exHIPAddressRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(ExHIPAddress exHIPAddress)
        {
            return exHIPAddressRepository.Get(chk => chk.ExchangeHouseID == exHIPAddress.ExchangeHouseID && chk.IPAddress == exHIPAddress.IPAddress) == null ? false : true;
        }

        public bool CreateExHIPAddress(ExHIPAddress exHIPAddress)
        {
            bool isSuccess = true;
            try
            {
                exHIPAddressRepository.Add(exHIPAddress);
                this.SaveRecord();
                ServiceUtil<ExHIPAddress>.WriteActionLog(exHIPAddress.Id, ENUMOperation.CREATE, exHIPAddress);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating ExHIPAddress", ex);
            }
            return isSuccess;
        }

        public bool UpdateExHIPAddress(ExHIPAddress exHIPAddress)
        {
            bool isSuccess = true;
            try
            {
                exHIPAddressRepository.Update(exHIPAddress);
                this.SaveRecord();
                ServiceUtil<ExHIPAddress>.WriteActionLog(exHIPAddress.Id, ENUMOperation.UPDATE, exHIPAddress);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating ExHIPAddress", ex);
            }
            return isSuccess;
        }

        public bool DeleteExHIPAddress(int id)
        {
            bool isSuccess = true;
            var exHIPAddress = exHIPAddressRepository.GetById(id);
            try
            {
                exHIPAddressRepository.Delete(exHIPAddress);
                SaveRecord();
                ServiceUtil<ExHIPAddress>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting ExHIPAddress", ex);
            }
            return isSuccess;
        }

        public ExHIPAddress GetExHIPAddress(int id)
        {
            return exHIPAddressRepository.GetById(id);
        }

        public IEnumerable<ExHIPAddress> GetAllExHIPAddress()
        {
            return exHIPAddressRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
