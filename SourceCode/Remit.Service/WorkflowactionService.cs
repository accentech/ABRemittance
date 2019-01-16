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
    public interface IWorkflowactionService
    {

        bool CreateWorkflowaction(Workflowaction workflowaction);
        bool UpdateWorkflowaction(Workflowaction workflowaction);
        bool DeleteWorkflowaction(int id);
        Workflowaction GetWorkflowaction(int id);

        IEnumerable<Workflowaction> GetAllWorkflowaction();
        void SaveRecord();

        bool CheckIsExist(Workflowaction workflowaction);
    }

    public class WorkflowactionService : IWorkflowactionService
    {
        private readonly IWorkflowactionRepository workflowactionRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(WorkflowactionService));

        public WorkflowactionService()
        {
        }

        public WorkflowactionService(IWorkflowactionRepository workflowactionRepository, IUnitOfWork unitOfWork)
        {
            this.workflowactionRepository = workflowactionRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CheckIsExist(Workflowaction workflowaction)
        {
            return false;
            //return workflowactionRepository.Get(chk => chk.Name == workflowaction.Name) == null ? false : true;
        }

        public bool CreateWorkflowaction(Workflowaction workflowaction)
        {
            bool isSuccess = true;
            try
            {
                workflowactionRepository.Add(workflowaction);
                this.SaveRecord();
                ServiceUtil<Workflowaction>.WriteActionLog(workflowaction.Id, ENUMOperation.CREATE, workflowaction);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Workflowaction", ex);
            }
            return isSuccess;
        }

        public bool UpdateWorkflowaction(Workflowaction workflowaction)
        {
            bool isSuccess = true;
            try
            {
                workflowactionRepository.Update(workflowaction);
                this.SaveRecord();
                //ServiceUtil<Workflowaction>.WriteActionLog(workflowaction.Id, ENUMOperation.UPDATE, workflowaction);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Workflowaction", ex);
            }
            return isSuccess;
        }

        public bool DeleteWorkflowaction(int id)
        {
            bool isSuccess = true;
            var workflowaction = workflowactionRepository.GetById(id);
            try
            {
                workflowactionRepository.Delete(workflowaction);
                SaveRecord();
                //ServiceUtil<Workflowaction>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Workflowaction", ex);
            }
            return isSuccess;
        }

        public Workflowaction GetWorkflowaction(int id)
        {
            return workflowactionRepository.GetById(id);
        }

        public IEnumerable<Workflowaction> GetAllWorkflowaction()
        {
            return workflowactionRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
