using Remit.Core.Common;
using Remit.Data.Repository;
using Remit.Data.Infrastructure;
using Remit.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.LoggerService;

namespace Remit.Service
{
    public interface IRoleSubModuleItemService
    {

        bool CreateRoleSubModuleItem(RoleSubModuleItem roleSubModuleItem);
        bool UpdateRoleSubModuleItem(RoleSubModuleItem roleSubModuleItem);
        bool DeleteRoleSubModuleItem(int id);
        IEnumerable<RoleSubModuleItem> GetRoleSubModuleItemByRole(int id);
        RoleSubModuleItem GetRoleSubModuleItem(int id);
        RoleSubModuleItem GetRoleSubModuleItemBySubModuleIdandRole(string url, int? roleId);
        IEnumerable<RoleSubModuleItem> GetAllRoleSubModuleItem();
        void SaveRecord();
    }
    public class RoleSubModuleItemService : IRoleSubModuleItemService
    {
        public RoleSubModuleItemService()
        {

        }
        private readonly IRoleSubModuleItemRepository roleSubModuleItemRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ISubModuleItemRepository subModuleItemRepository;
        private readonly LoggingService logger = new LoggingService(typeof(RoleSubModuleItemService));

        public RoleSubModuleItemService(IRoleSubModuleItemRepository roleSubModuleItemRepository, ISubModuleItemRepository subModuleItemRepository, IUnitOfWork unitOfWork)
        {
            this.roleSubModuleItemRepository = roleSubModuleItemRepository;
            this.unitOfWork = unitOfWork;
            this.subModuleItemRepository = subModuleItemRepository;
        }


        public bool CreateRoleSubModuleItem(RoleSubModuleItem roleSubModuleItem)
        {
            bool isSuccess = true;
            try
            {
                roleSubModuleItemRepository.Add(roleSubModuleItem);
                this.SaveRecord();
                ServiceUtil<RoleSubModuleItem>.WriteActionLog(roleSubModuleItem.Id, ENUMOperation.CREATE, roleSubModuleItem);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating RoleSubModuleItem", ex);
            }
            return isSuccess;
        }

        public bool UpdateRoleSubModuleItem(RoleSubModuleItem roleSubModuleItem)
        {
            bool isSuccess = true;
            try
            {
                roleSubModuleItemRepository.Update(roleSubModuleItem);
                this.SaveRecord();
                ServiceUtil<RoleSubModuleItem>.WriteActionLog(roleSubModuleItem.Id, ENUMOperation.UPDATE, roleSubModuleItem);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating RoleSubModuleItem", ex);
            }
            return isSuccess;
        }

        public bool DeleteRoleSubModuleItem(int id)
        {
            bool isSuccess = true;
            var roleSubModuleItem = roleSubModuleItemRepository.GetById(id);
            try
            {
                roleSubModuleItemRepository.Delete(roleSubModuleItem);
                SaveRecord();
                ServiceUtil<RoleSubModuleItem>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting RoleSubModuleItem", ex);
            }
            return isSuccess;
        }

        public RoleSubModuleItem GetRoleSubModuleItem(int id)
        {
            return roleSubModuleItemRepository.GetById(id);
        }

        public IEnumerable<RoleSubModuleItem> GetRoleSubModuleItemByRole(int id)
        {
            return roleSubModuleItemRepository.GetMany(role=> role.RoleId == id);
        }

        public RoleSubModuleItem GetRoleSubModuleItemBySubModuleIdandRole(string url, int? roleId)
        {
            var getSubModuleItem = subModuleItemRepository.Get(sub => sub.UrlPath == url);
            if(getSubModuleItem != null)
                return roleSubModuleItemRepository.Get(item => item.RoleId == roleId && item.SubModuleItemId == getSubModuleItem.Id);
            return null;
        }

        public IEnumerable<RoleSubModuleItem> GetAllRoleSubModuleItem()
        {
            return roleSubModuleItemRepository.GetAll();
        }
        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
