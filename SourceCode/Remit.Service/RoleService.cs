using Remit.Data.Repository;
using Remit.Data.Infrastructure;
using Remit.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.LoggerService;
using Remit.Core.Common;

namespace Remit.Service
{
    public interface IRoleService
    {

        bool CreateRole(Role role);
        bool UpdateRole(Role role);
        bool DeleteRole(int id);
        Role GetRole(int id);
        
        IEnumerable<Role> GetAllRole();
        void SaveRecord();
        bool CheckIsExist(Role role);
    }
    public class RoleService : IRoleService
    {
        public RoleService()
        {

        }
        private readonly IRoleRepository roleRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(RoleService));

        public RoleService(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
        {
            this.roleRepository = roleRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CheckIsExist(Role role)
        {
            return roleRepository.Get(chk => chk.Name == role.Name) == null ? false : true;
        }

        public bool CreateRole(Role role)
        {
            bool isSuccess = true;
            try
            {
                roleRepository.Add(role);
                this.SaveRecord();
                ServiceUtil<Role>.WriteActionLog(role.Id, ENUMOperation.CREATE, role);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Role", ex);
            }
            return isSuccess;
        }

        public bool UpdateRole(Role role)
        {
            bool isSuccess = true;
            try
            {
                roleRepository.Update(role);
                this.SaveRecord();
                ServiceUtil<Role>.WriteActionLog(role.Id, ENUMOperation.UPDATE, role);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Role", ex);
            }
            return isSuccess;
        }

        public bool DeleteRole(int id)
        {
            bool isSuccess = true;
            var role = roleRepository.GetById(id);
            try
            {
                roleRepository.Delete(role);
                SaveRecord();
                ServiceUtil<Role>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Role", ex);
            }
            return isSuccess;
        }

        public Role GetRole(int id)
        {
            return roleRepository.GetById(id);
        }
        
        
        public IEnumerable<Role> GetAllRole()
        {
            return roleRepository.GetAll();
        }
        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
