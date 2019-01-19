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
    public interface IAgentService
    {
        bool CreateAgent(Agent agent);
        bool UpdateAgent(Agent agent);
        
        bool DeleteAgent(int id);
        Agent GetAgent(int id);
        IEnumerable<Agent> GetAllAgent();
        void SaveRecord();
       
        bool CheckIsExist(Agent agent);
        
    }

    public class AgentService : IAgentService
    {
        private readonly IAgentRepository agentRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(AgentService));

        public AgentService()
        {
        }
        public AgentService(IAgentRepository agentRepository, IUnitOfWork unitOfWork)
        {

            this.agentRepository = agentRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(Agent agent)
        {

            return agentRepository.Get(chk => chk.AgentName == agent.AgentName) == null ? false : true;
        }

        public bool CreateAgent(Agent agent)
        {
            bool isSuccess = true;
            try
            {
                agentRepository.Add(agent);
                this.SaveRecord();
                ServiceUtil<Agent>.WriteActionLog(agent.Id, ENUMOperation.CREATE, agent);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Agent", ex);
            }
            return isSuccess;
        }

        public bool UpdateAgent(Agent agent)
        {
            bool isSuccess = true;
            try
            {
                agentRepository.Update(agent);
                this.SaveRecord();
                ServiceUtil<Agent>.WriteActionLog(agent.Id, ENUMOperation.UPDATE, agent);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Agent", ex);
            }
            return isSuccess;
        }

        public bool DeleteAgent(int id)
        {
            bool isSuccess = true;
            var agent = agentRepository.GetById(id);
            try
            {
                agentRepository.Delete(agent);
                SaveRecord();
                ServiceUtil<Agent>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Agent", ex);
            }
            return isSuccess;
        }

        public Agent GetAgent(int id)
        {
            return agentRepository.GetById(id);
        }

        public IEnumerable<Agent> GetAllAgent()
        {
            return agentRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
