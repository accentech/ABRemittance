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
    public interface IExHRemitDataService
    {
        bool CreateExHRemitData(ExHRemitData exHRemitData);
        bool UpdateExHRemitData(ExHRemitData exHRemitData);
        
        bool DeleteExHRemitData(int id);
        ExHRemitData GetExHRemitData(int id);
        IEnumerable<ExHRemitData> GetAllExHRemitData();
        void SaveRecord();
       
        bool CheckIsExist(ExHRemitData exHRemitData);
        
    }

    public class ExHRemitDataService : IExHRemitDataService
    {
        private readonly IExHRemitDataRepository exHRemitDataRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(ExHRemitDataService));

        public ExHRemitDataService()
        {
        }
        public ExHRemitDataService(IExHRemitDataRepository exHRemitDataRepository, IUnitOfWork unitOfWork)
        {

            this.exHRemitDataRepository = exHRemitDataRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(ExHRemitData exHRemitData)
        {
            return exHRemitDataRepository.Get(chk => chk.ExchangeHouseId == exHRemitData.ExchangeHouseId && chk.DataParsedBy==exHRemitData.DataParsedBy && chk.DataParsingDate==exHRemitData.DataParsingDate && chk.DataParsingStatus==exHRemitData.DataParsingStatus) == null ? false : true;
        }

        public bool CreateExHRemitData(ExHRemitData exHRemitData)
        {
            bool isSuccess = true;
            try
            {
                exHRemitDataRepository.Add(exHRemitData);
                this.SaveRecord();
                ServiceUtil<ExHRemitData>.WriteActionLog(exHRemitData.Id, ENUMOperation.CREATE, exHRemitData);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating ExHRemitData", ex);
            }
            return isSuccess;
        }

        public bool UpdateExHRemitData(ExHRemitData exHRemitData)
        {
            bool isSuccess = true;
            try
            {
                exHRemitDataRepository.Update(exHRemitData);
                this.SaveRecord();
                ServiceUtil<ExHRemitData>.WriteActionLog(exHRemitData.Id, ENUMOperation.UPDATE, exHRemitData);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating ExHRemitData", ex);
            }
            return isSuccess;
        }

        public bool DeleteExHRemitData(int id)
        {
            bool isSuccess = true;
            var exHRemitData = exHRemitDataRepository.GetById(id);
            try
            {
                exHRemitDataRepository.Delete(exHRemitData);
                SaveRecord();
                ServiceUtil<ExHRemitData>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting ExHRemitData", ex);
            }
            return isSuccess;
        }

        public ExHRemitData GetExHRemitData(int id)
        {
            return exHRemitDataRepository.GetById(id);
        }

        public IEnumerable<ExHRemitData> GetAllExHRemitData()
        {
            return exHRemitDataRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
