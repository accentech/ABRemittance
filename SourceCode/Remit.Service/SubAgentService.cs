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
    public interface ISubAgentService
    {
        bool CreateSubAgent(SubAgent subAgent);
        bool UpdateSubAgent(SubAgent subAgent);
        
        bool DeleteSubAgent(int id);
        SubAgent GetSubAgent(int id);
        IEnumerable<SubAgent> GetAllSubAgent();
        void SaveRecord();
       
        bool CheckIsExist(SubAgent subAgent);
        
    }

    public class SubAgentService : ISubAgentService
    {
        private readonly ISubAgentRepository subAgentRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(SubAgentService));

        public SubAgentService()
        {
        }
        public SubAgentService(ISubAgentRepository subAgentRepository, IUnitOfWork unitOfWork)
        {

            this.subAgentRepository = subAgentRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(SubAgent subAgent)
        {

            return subAgentRepository.Get(chk => chk.AgentId == subAgent.AgentId && chk.LocationName == subAgent.LocationName) == null ? false : true;
        }

        public bool CreateSubAgent(SubAgent subAgent)
        {
            bool isSuccess = true;
            try
            {
                subAgentRepository.Add(subAgent);
                this.SaveRecord();
                ServiceUtil<SubAgent>.WriteActionLog(subAgent.Id, ENUMOperation.CREATE, subAgent);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating SubAgent", ex);
            }
            return isSuccess;
        }

        public bool UpdateSubAgent(SubAgent subAgent)
        {
            bool isSuccess = true;
            try
            {
                subAgentRepository.Update(subAgent);
                this.SaveRecord();
                ServiceUtil<SubAgent>.WriteActionLog(subAgent.Id, ENUMOperation.UPDATE, subAgent);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating SubAgent", ex);
            }
            return isSuccess;
        }

        public bool DeleteSubAgent(int id)
        {
            bool isSuccess = true;
            var subAgent = subAgentRepository.GetById(id);
            try
            {
                subAgentRepository.Delete(subAgent);
                SaveRecord();
                ServiceUtil<SubAgent>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting SubAgent", ex);
            }
            return isSuccess;
        }

        public SubAgent GetSubAgent(int id)
        {
            return subAgentRepository.GetById(id);
        }

        public IEnumerable<SubAgent> GetAllSubAgent()
        {
            return subAgentRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
