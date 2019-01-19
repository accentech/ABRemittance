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
    public interface IExHRemitDataDetailService
    {
        bool CreateExHRemitDataDetail(ExHRemitDataDetail exHRemitDataDetail);
        bool UpdateExHRemitDataDetail(ExHRemitDataDetail exHRemitDataDetail);
        
        bool DeleteExHRemitDataDetail(int id);
        ExHRemitDataDetail GetExHRemitDataDetail(int id);
        IEnumerable<ExHRemitDataDetail> GetAllExHRemitDataDetail();
        void SaveRecord();
       
        bool CheckIsExist(ExHRemitDataDetail exHRemitDataDetail);
        
    }

    public class ExHRemitDataDetailService : IExHRemitDataDetailService
    {
        private readonly IExHRemitDataDetailRepository exHRemitDataDetailRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(ExHRemitDataDetailService));

        public ExHRemitDataDetailService()
        {
        }
        public ExHRemitDataDetailService(IExHRemitDataDetailRepository exHRemitDataDetailRepository, IUnitOfWork unitOfWork)
        {

            this.exHRemitDataDetailRepository = exHRemitDataDetailRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(ExHRemitDataDetail exHRemitDataDetail)
        {
            return exHRemitDataDetailRepository.Get(chk => chk.ExHRemitDataId == exHRemitDataDetail.ExHRemitDataId && chk.ParsedStatus == exHRemitDataDetail.ParsedStatus && chk.RawRemitData == exHRemitDataDetail.RawRemitData) == null ? false : true;
        }

        public bool CreateExHRemitDataDetail(ExHRemitDataDetail exHRemitDataDetail)
        {
            bool isSuccess = true;
            try
            {
                exHRemitDataDetailRepository.Add(exHRemitDataDetail);
                this.SaveRecord();
                ServiceUtil<ExHRemitDataDetail>.WriteActionLog(exHRemitDataDetail.Id, ENUMOperation.CREATE, exHRemitDataDetail);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating ExHRemitDataDetail", ex);
            }
            return isSuccess;
        }

        public bool UpdateExHRemitDataDetail(ExHRemitDataDetail exHRemitDataDetail)
        {
            bool isSuccess = true;
            try
            {
                exHRemitDataDetailRepository.Update(exHRemitDataDetail);
                this.SaveRecord();
                ServiceUtil<ExHRemitDataDetail>.WriteActionLog(exHRemitDataDetail.Id, ENUMOperation.UPDATE, exHRemitDataDetail);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating ExHRemitDataDetail", ex);
            }
            return isSuccess;
        }

        public bool DeleteExHRemitDataDetail(int id)
        {
            bool isSuccess = true;
            var exHRemitDataDetail = exHRemitDataDetailRepository.GetById(id);
            try
            {
                exHRemitDataDetailRepository.Delete(exHRemitDataDetail);
                SaveRecord();
                ServiceUtil<ExHRemitDataDetail>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting ExHRemitDataDetail", ex);
            }
            return isSuccess;
        }

        public ExHRemitDataDetail GetExHRemitDataDetail(int id)
        {
            return exHRemitDataDetailRepository.GetById(id);
        }

        public IEnumerable<ExHRemitDataDetail> GetAllExHRemitDataDetail()
        {
            return exHRemitDataDetailRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
