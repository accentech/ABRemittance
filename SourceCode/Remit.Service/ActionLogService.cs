using Remit.Data.Repository;
using Remit.Data.Infrastructure;
using Remit.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Remit.Service
{
    public interface IActionLogService
    {

        bool CreateActionLog(ActionLog actionLog);
        bool UpdateActionLog(ActionLog actionLog);
        bool DeleteActionLog(Guid id);
        ActionLog GetActionLog(Guid id);
        ActionLog GetActionLogByName(ActionLog actionLog);
        IEnumerable<ActionLog> GetAllActionLog();
        void SaveRecord();
    }
    public class ActionLogService : IActionLogService
    {
        public ActionLogService()
        {

        }
        private readonly IActionLogRepository actionLogRepository;
        private readonly IUnitOfWork unitOfWork;        

        public ActionLogService(IActionLogRepository actionLogRepository, IUnitOfWork unitOfWork)
        {
            this.actionLogRepository = actionLogRepository;
            this.unitOfWork = unitOfWork;
            
        }
        public bool CreateActionLog(ActionLog actionLog)
        {
            bool isSuccess = true;
            try
            {
                actionLogRepository.Add(actionLog);
                this.SaveRecord();

            }
            catch
            {
                isSuccess = false;
            }
            return isSuccess;
        }

        public bool UpdateActionLog(ActionLog actionLog)
        {
            bool isSuccess = true;
            try
            {
                actionLogRepository.Update(actionLog);
                this.SaveRecord();
            }
            catch
            {
                isSuccess = false;
            }
            return isSuccess;
        }

        public bool DeleteActionLog(Guid id)
        {
            bool isSuccess = true;
            var actionLog = actionLogRepository.GetById(id);
            try
            {
                actionLogRepository.Delete(actionLog);
                SaveRecord();
            }
            catch
            {
                isSuccess = false;
            }
            return isSuccess;
        }

        public ActionLog GetActionLog(Guid id)
        {
            return actionLogRepository.GetById(id);
        }
        public ActionLog GetActionLogByName(ActionLog actionLog)
        {
            return actionLogRepository.Get(u => u.ActionCRUD == actionLog.ActionCRUD);
        }


        public IEnumerable<ActionLog> GetAllActionLog()
        {
            return actionLogRepository.GetAll();
        }
        public void SaveRecord()
        {
            unitOfWork.Commit();
        }


    }
}
