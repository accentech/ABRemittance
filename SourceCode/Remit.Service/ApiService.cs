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
    public interface IApiService
    {
        bool CreateApi(Api api);
        bool UpdateApi(Api api);
        
        bool DeleteApi(int id);
        Api GetApi(int id);
        IEnumerable<Api> GetAllApi();
        void SaveRecord();
       
        bool CheckIsExist(Api api);
        
    }

    public class ApiService : IApiService
    {
        private readonly IApiRepository apiRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(ApiService));

        public ApiService()
        {
        }
        public ApiService(IApiRepository apiRepository, IUnitOfWork unitOfWork)
        {

            this.apiRepository = apiRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(Api api)
        {

            return apiRepository.Get(chk => chk.APIName == api.APIName) == null ? false : true;
        }

        public bool CreateApi(Api api)
        {
            bool isSuccess = true;
            try
            {
                apiRepository.Add(api);
                this.SaveRecord();
                ServiceUtil<Api>.WriteActionLog(api.Id, ENUMOperation.CREATE, api);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Api", ex);
            }
            return isSuccess;
        }

        public bool UpdateApi(Api api)
        {
            bool isSuccess = true;
            try
            {
                apiRepository.Update(api);
                this.SaveRecord();
                ServiceUtil<Api>.WriteActionLog(api.Id, ENUMOperation.UPDATE, api);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Api", ex);
            }
            return isSuccess;
        }

        public bool DeleteApi(int id)
        {
            bool isSuccess = true;
            var api = apiRepository.GetById(id);
            try
            {
                apiRepository.Delete(api);
                SaveRecord();
                ServiceUtil<Api>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Api", ex);
            }
            return isSuccess;
        }

        public Api GetApi(int id)
        {
            return apiRepository.GetById(id);
        }

        public IEnumerable<Api> GetAllApi()
        {
            return apiRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
