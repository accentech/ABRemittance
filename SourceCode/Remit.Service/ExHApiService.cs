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
    public interface IExHApiService
    {
        bool CreateExHApi(ExHApi exHApi);
        bool UpdateExHApi(ExHApi exHApi);
        
        bool DeleteExHApi(int id);
        ExHApi GetExHApi(int id);
        IEnumerable<ExHApi> GetAllExHApi();
        void SaveRecord();
       
        bool CheckIsExist(ExHApi exHApi);
        
    }

    public class ExHApiService : IExHApiService
    {
        private readonly IExHApiRepository exHApiRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(ExHApiService));

        public ExHApiService()
        {
        }
        public ExHApiService(IExHApiRepository exHApiRepository, IUnitOfWork unitOfWork)
        {

            this.exHApiRepository = exHApiRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(ExHApi exHApi)
        {

            return exHApiRepository.Get(chk => chk.APIId == exHApi.APIId && chk.ExchangeHouseId == exHApi.ExchangeHouseId) == null ? false : true;
        }

        public bool CreateExHApi(ExHApi exHApi)
        {
            bool isSuccess = true;
            try
            {
                exHApiRepository.Add(exHApi);
                this.SaveRecord();
                ServiceUtil<ExHApi>.WriteActionLog(exHApi.Id, ENUMOperation.CREATE, exHApi);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating ExHApi", ex);
            }
            return isSuccess;
        }

        public bool UpdateExHApi(ExHApi exHApi)
        {
            bool isSuccess = true;
            try
            {
                exHApiRepository.Update(exHApi);
                this.SaveRecord();
                ServiceUtil<ExHApi>.WriteActionLog(exHApi.Id, ENUMOperation.UPDATE, exHApi);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating ExHApi", ex);
            }
            return isSuccess;
        }

        public bool DeleteExHApi(int id)
        {
            bool isSuccess = true;
            var exHApi = exHApiRepository.GetById(id);
            try
            {
                exHApiRepository.Delete(exHApi);
                SaveRecord();
                ServiceUtil<ExHApi>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting ExHApi", ex);
            }
            return isSuccess;
        }

        public ExHApi GetExHApi(int id)
        {
            return exHApiRepository.GetById(id);
        }

        public IEnumerable<ExHApi> GetAllExHApi()
        {
            return exHApiRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
