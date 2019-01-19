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
    public interface IWorkflowactionSettingService
    {

        bool CreateWorkflowactionSetting(WorkflowactionSetting workflowactionSetting);
        bool UpdateWorkflowactionSetting(WorkflowactionSetting workflowactionSetting);
        bool DeleteWorkflowactionSetting(Guid id);
        bool DeleteAllWorkflowactionSettingBySubModuleId(int subModuleItemId, int workflowactionId);
        WorkflowactionSetting GetWorkflowactionSetting(Guid id);
        bool CheckUserInWorkflowactionSettingForUrl(string urlPath, int userId, int workflowactionId);
        IEnumerable<WorkflowactionSetting> GetAllWorkflowactionSetting();
        IEnumerable<WorkflowactionSetting> GetWorkflowactionSettingBySubModuleItemId(int subModuleItemId, int workflowactionId);
        IEnumerable<WorkflowactionSetting> GetWorkflowactionSettingByEmployeeId(int employeeId, int workflowaction);
        List<string> GetAllEmployeesinBySubModuleItemId(string url);
        List<string> GetAllEmployeesinBySubModuleItemId(string url, int action);

        void SaveRecord();
    }

    public class WorkflowactionSettingService : IWorkflowactionSettingService
    {
        private readonly IWorkflowactionSettingRepository workflowactionSettingRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(WorkflowactionSettingService));

        public WorkflowactionSettingService()
        {
        }

        public WorkflowactionSettingService(IWorkflowactionSettingRepository workflowactionSettingRepository, IUnitOfWork unitOfWork)
        {
            this.workflowactionSettingRepository = workflowactionSettingRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CreateWorkflowactionSetting(WorkflowactionSetting workflowactionSetting)
        {
            bool isSuccess = true;
            try
            {
                workflowactionSettingRepository.Add(workflowactionSetting);
                this.SaveRecord();
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating WorkflowactionSetting", ex);
            }
            return isSuccess;
        }

        public bool UpdateWorkflowactionSetting(WorkflowactionSetting workflowactionSetting)
        {
            bool isSuccess = true;
            try
            {
                workflowactionSettingRepository.Update(workflowactionSetting);
                this.SaveRecord();
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating WorkflowactionSetting", ex);
            }
            return isSuccess;
        }

        public bool DeleteWorkflowactionSetting(Guid id)
        {
            bool isSuccess = true;
            var workflowactionSetting = workflowactionSettingRepository.GetById(id);
            try
            {
                workflowactionSettingRepository.Delete(workflowactionSetting);
                SaveRecord();
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting WorkflowactionSetting", ex);
            }
            return isSuccess;
        }

        public bool DeleteAllWorkflowactionSettingBySubModuleId(int subModuleItemId, int workflowactionId)
        {
            bool isSuccess = true;
            var workflowactionSettingListObj = workflowactionSettingRepository.GetMany(ap => ap.SubMouduleItemId == subModuleItemId && ap.WorkflowactionId == workflowactionId);

            if (workflowactionSettingListObj != null)
            {
                try
                {
                    foreach (var workflowactionSetting in workflowactionSettingListObj)
                    {
                        workflowactionSettingRepository.Delete(workflowactionSetting);
                        SaveRecord();
                        ServiceUtil<WorkflowactionSetting>.WriteActionLog(workflowactionSetting.Id, ENUMOperation.DELETE);
                    }
                }
                catch (Exception ex)
                {
                    isSuccess = false;
                    logger.Error("Error in deleting WorkflowactionSetting", ex);
                }
            }
            return isSuccess;
        }

        public WorkflowactionSetting GetWorkflowactionSetting(Guid id)
        {
            return workflowactionSettingRepository.GetById(id);
        }

        public IEnumerable<WorkflowactionSetting> GetAllWorkflowactionSetting()
        {
            return workflowactionSettingRepository.GetAll();
        }

        public IEnumerable<WorkflowactionSetting> GetWorkflowactionSettingBySubModuleItemId(int subModuleItemId, int workflowactionId)
        {
            return workflowactionSettingRepository.GetMany(ap => ap.SubMouduleItemId == subModuleItemId && ap.WorkflowactionId == workflowactionId);
        }

        public IEnumerable<WorkflowactionSetting> GetWorkflowactionSettingByEmployeeId(int employeeId, int workflowaction)
        {
            return workflowactionSettingRepository.GetMany(ap => ap.EmployeeId == employeeId && ap.WorkflowactionId == workflowaction);
        }

        public List<string> GetAllEmployeesinBySubModuleItemId(string url)
        {
            var emailAddresses = workflowactionSettingRepository.GetMany(ap => ap.SubModuleItem.UrlPath == url).Select(ap => ap.Employee.Email).Distinct().ToList();
            return emailAddresses;
        }

        public List<string> GetAllEmployeesinBySubModuleItemId(string url, int action)
        {
            var emailAddresses = workflowactionSettingRepository.GetMany(ap => ap.WorkflowactionId == action && ap.SubModuleItem.UrlPath == url).Select(ap => ap.Employee.Email).Distinct().ToList();
            return emailAddresses;
        }

        public bool CheckUserInWorkflowactionSettingForUrl(string urlPath, int userId, int workflowactionId)
        {
            var workflowactionSettingObj = new List<WorkflowactionSetting>();// this.workflowactionSettingRepository.Get(ap => ap.SubModuleItem.UrlPath == urlPath && ap.WorkflowactionId == workflowactionId && ap.Employee.BusinessUsers.Any(u => u.Id == userId));
            if (workflowactionSettingObj == null)
                return false;
            return true;
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
