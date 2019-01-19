using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Core.Common;
using Remit.Data.Infrastructure;
using Remit.Data.Repository;
using Remit.LoggerService;
using Remit.Model.Models;

namespace Remit.Service
{
    public interface INotificationSettingService
    {

        bool CreateNotificationSetting(NotificationSetting notificationSetting);
        bool UpdateNotificationSetting(NotificationSetting notificationSetting);
        bool DeleteNotificationSetting(Guid id);
        NotificationSetting GetNotificationSetting(Guid id);
        IEnumerable<NotificationSetting> GetAllNotificationSetting();
        IEnumerable<NotificationSetting> GetNotificationSettingOfASubModule(int subModuleItemId);
        bool DeleteAllNotificationSettingBySubModuleId(int subModuleItemId);
        bool DeleteAllNotificationSettingBySubModuleId(int subModuleItemId, int workflowactionId);
        List<string> GetAllEmployeesinBySubModuleItemId(string url, int action);

        void SaveRecord();
    }

    public class NotificationSettingService : INotificationSettingService
    {
        private readonly INotificationSettingRepository notificationSettingRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(NotificationSettingService));

        public NotificationSettingService()
        {
        }

        public NotificationSettingService(INotificationSettingRepository notificationSettingRepository, IUnitOfWork unitOfWork)
        {
            this.notificationSettingRepository = notificationSettingRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CreateNotificationSetting(NotificationSetting notificationSetting)
        {
            bool isSuccess = true;
            try
            {
                notificationSettingRepository.Add(notificationSetting);
                this.SaveRecord();
                ServiceUtil<NotificationSetting>.WriteActionLog(notificationSetting.Id, ENUMOperation.CREATE, notificationSetting);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating NotificationSetting", ex);
            }
            return isSuccess;
        }

        public bool UpdateNotificationSetting(NotificationSetting notificationSetting)
        {
            bool isSuccess = true;
            try
            {
                notificationSettingRepository.Update(notificationSetting);
                this.SaveRecord();
                ServiceUtil<NotificationSetting>.WriteActionLog(notificationSetting.Id, ENUMOperation.UPDATE, notificationSetting);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating NotificationSetting", ex);
            }
            return isSuccess;
        }

        public bool DeleteNotificationSetting(Guid id)
        {
            bool isSuccess = true;
            var notificationSetting = notificationSettingRepository.GetById(id);
            try
            {
                notificationSettingRepository.Delete(notificationSetting);
                SaveRecord();
                ServiceUtil<NotificationSetting>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting NotificationSetting", ex);
            }
            return isSuccess;
        }

        public NotificationSetting GetNotificationSetting(Guid id)
        {
            return notificationSettingRepository.GetById(id);
        }

        public IEnumerable<NotificationSetting> GetAllNotificationSetting()
        {
            return notificationSettingRepository.GetAll();
        }

        public IEnumerable<NotificationSetting> GetNotificationSettingOfASubModule(int subModuleItemId)
        {
            return notificationSettingRepository.GetMany(ap => ap.SubModuleItemId == subModuleItemId);
        }
        public bool DeleteAllNotificationSettingBySubModuleId(int subModuleItemId)
        {
            bool isSuccess = true;
            var notificationSettingListObj = notificationSettingRepository.GetMany(ap => ap.SubModuleItemId == subModuleItemId);

            if (notificationSettingListObj != null)
            {
                try
                {
                    foreach (var notificationSetting in notificationSettingListObj)
                    {
                        notificationSettingRepository.Delete(notificationSetting);
                        SaveRecord();
                        ServiceUtil<NotificationSetting>.WriteActionLog(notificationSetting.Id, ENUMOperation.DELETE);
                    }
                }
                catch (Exception ex)
                {
                    isSuccess = false;
                    logger.Error("Error in deleting NotificationSetting", ex);
                }
            }
            return isSuccess;
        }
        public bool DeleteAllNotificationSettingBySubModuleId(int subModuleItemId, int workflowactionId)
        {
            bool isSuccess = true;
            var notificationSettingListObj = notificationSettingRepository.GetMany(ap => ap.SubModuleItemId == subModuleItemId && ap.WorkflowactionId == workflowactionId);

            if (notificationSettingListObj != null)
            {
                try
                {
                    foreach (var notificationSetting in notificationSettingListObj)
                    {
                        notificationSettingRepository.Delete(notificationSetting);
                        SaveRecord();
                        ServiceUtil<NotificationSetting>.WriteActionLog(notificationSetting.Id, ENUMOperation.DELETE);
                    }
                }
                catch (Exception ex)
                {
                    isSuccess = false;
                    logger.Error("Error in deleting NotificationSetting", ex);
                }
            }
            return isSuccess;
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }


        public List<string> GetAllEmployeesinBySubModuleItemId(string url, int action)
        {
            var emailAddresses = notificationSettingRepository.GetMany(ap => ap.WorkflowactionId == action && ap.SubModuleItem.UrlPath == url).Select(ap => ap.Employee.Email).Distinct().ToList();
            return emailAddresses;
        }
    }
}
