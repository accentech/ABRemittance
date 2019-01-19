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
    public interface IExHUserService
    {
        bool CreateExHUser(ExHUser exHUser);
        bool UpdateExHUser(ExHUser exHUser);
        
        bool DeleteExHUser(int id);
        ExHUser GetExHUser(int id);
        IEnumerable<ExHUser> GetAllExHUser();
        void SaveRecord();
       
        bool CheckIsExist(ExHUser exHUser);
        
    }

    public class ExHUserService : IExHUserService
    {
        private readonly IExHUserRepository exHUserRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(ExHUserService));

        public ExHUserService()
        {
        }
        public ExHUserService(IExHUserRepository exHUserRepository, IUnitOfWork unitOfWork)
        {

            this.exHUserRepository = exHUserRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(ExHUser exHUser)
        {

            return exHUserRepository.Get(chk => chk.ExUserName == exHUser.ExUserName) == null ? false : true;
        }

        public bool CreateExHUser(ExHUser exHUser)
        {
            bool isSuccess = true;
            try
            {
                exHUserRepository.Add(exHUser);
                this.SaveRecord();
                ServiceUtil<ExHUser>.WriteActionLog(exHUser.Id, ENUMOperation.CREATE, exHUser);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating ExHUser", ex);
            }
            return isSuccess;
        }

        public bool UpdateExHUser(ExHUser exHUser)
        {
            bool isSuccess = true;
            try
            {
                exHUserRepository.Update(exHUser);
                this.SaveRecord();
                ServiceUtil<ExHUser>.WriteActionLog(exHUser.Id, ENUMOperation.UPDATE, exHUser);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating ExHUser", ex);
            }
            return isSuccess;
        }

        public bool DeleteExHUser(int id)
        {
            bool isSuccess = true;
            var exHUser = exHUserRepository.GetById(id);
            try
            {
                exHUserRepository.Delete(exHUser);
                SaveRecord();
                ServiceUtil<ExHUser>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting ExHUser", ex);
            }
            return isSuccess;
        }

        public ExHUser GetExHUser(int id)
        {
            return exHUserRepository.GetById(id);
        }

        public IEnumerable<ExHUser> GetAllExHUser()
        {
            return exHUserRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
