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
    public interface IBranchService
    {

        bool CreateBranch(Branch branch);
        bool UpdateBranch(Branch branch);
        //Branch GetBranchByComponentId(int componentId);  
        bool DeleteBranch(int id);
        Branch GetBranch(int id);
        
        IEnumerable<Branch> GetAllBranch();
        void SaveRecord();
        //bool UpdateBalance(int compid, int quantity, bool isReduce);
        bool CheckIsExist(Branch branch);
        //Branch GetBranchByPartNo(string partNo);
    }

    public class BranchService : IBranchService
    {
        private readonly IBranchRepository branchRepository;
       
        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(BranchService));

        public BranchService()
        {
        }
        public BranchService(IBranchRepository branchRepository, IUnitOfWork unitOfWork)
        {
            
            this.branchRepository = branchRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(Branch branch)
        {
           return branchRepository.Get(chk => chk.Name == branch.Name) == null ? false : true;
        }
       
        public bool CreateBranch(Branch branch)
        {
            bool isSuccess = true;
            try
            {
                branchRepository.Add(branch);                
                this.SaveRecord();
                ServiceUtil<Branch>.WriteActionLog(branch.Id, ENUMOperation.CREATE, branch);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Branch", ex);
            }
            return isSuccess;
        }

        public bool UpdateBranch(Branch branch)
        {
            bool isSuccess = true;
            try
            {
                branchRepository.Update(branch);
                this.SaveRecord();
                ServiceUtil<Branch>.WriteActionLog(branch.Id, ENUMOperation.UPDATE, branch);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Branch", ex);
            }
            return isSuccess;
        }

        public bool DeleteBranch(int id)
        {
            bool isSuccess = true;
            var branch = branchRepository.GetById(id);
            try
            {
                branchRepository.Delete(branch);
                SaveRecord();
                ServiceUtil<Branch>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Branch", ex);
            }
            return isSuccess;
        }

        public Branch GetBranch(int id)
        {
            return branchRepository.GetById(id);
        }

      

       
               
        public IEnumerable<Branch> GetAllBranch()
        {
            return branchRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
