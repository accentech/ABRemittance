using Remit.Data.Repository;
using Remit.Data.Infrastructure;
using Remit.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.CachingService;
using Remit.LoggerService;
using Remit.Core.Common;

namespace Remit.Service
{
    public interface ISubModuleService
    {

        bool CreateSubModule(SubModule subModule);
        bool UpdateSubModule(SubModule subModule);
        bool DeleteSubModule(int id);
        SubModule GetSubModule(int id);
        IEnumerable<SubModule> GetAllSubModule();
        IEnumerable<SubModule> GetSubModulesByModuleIdAndRoleId(int moduleId, int roleId);

        void SaveRecord();
        bool CheckIsExist(SubModule subModule);
    }
    public class SubModuleService : ISubModuleService
    {
        public SubModuleService()
        {

        }
        private readonly ISubModuleRepository subModuleRepository;
        private readonly ISubModuleItemRepository subModuleItemRepository;
        private readonly IUnitOfWork unitOfWork;

        private readonly ICacheProvider cacheProvider = new DefaultCacheProvider();
        private readonly LoggingService logger = new LoggingService(typeof(SubModuleService));
        public SubModuleService(ISubModuleRepository subModuleRepository, IUnitOfWork unitOfWork, ISubModuleItemRepository subModuleItemRepository)
        {
            this.subModuleRepository = subModuleRepository;
            this.subModuleItemRepository = subModuleItemRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(SubModule subModule)
        {
            return subModuleRepository.Get(chk => chk.Name == subModule.Name && chk.ModuleId==subModule.ModuleId) == null ? false : true;
        }


        public bool CreateSubModule(SubModule subModule)
        {
            bool isSuccess = true;
            try
            {
                subModuleRepository.Add(subModule);
                this.SaveRecord();
                ServiceUtil<SubModule>.WriteActionLog(subModule.Id, ENUMOperation.CREATE, subModule);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating SubModule", ex);
            }
            return isSuccess;
        }

        public bool UpdateSubModule(SubModule subModule)
        {
            bool isSuccess = true;
            try
            {
                subModuleRepository.Update(subModule);
                this.SaveRecord();
                ServiceUtil<SubModule>.WriteActionLog(subModule.Id, ENUMOperation.UPDATE, subModule);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating SubModule", ex);
            }
            return isSuccess;
        }

        public bool DeleteSubModule(int id)
        {
            bool isSuccess = true;
            var subModule = subModuleRepository.GetById(id);
            try
            {
                subModuleRepository.Delete(subModule);
                SaveRecord();
                ServiceUtil<SubModule>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting SubModule", ex);
            }
            return isSuccess;
        }

        public SubModule GetSubModule(int id)
        {
            return subModuleRepository.GetById(id);
        }

        public IEnumerable<SubModule> GetAllSubModule()
        {
            return subModuleRepository.GetAll();
        }
        public IEnumerable<SubModule> GetSubModulesByModuleId(int moduleId)
        {
            return subModuleRepository.GetAll();
        }

        public IEnumerable<SubModule> GetSubModulesByModuleIdAndRoleId(int moduleId, int roleId)
        {

            IEnumerable<SubModule> subModuleList = cacheProvider.Get("submodule" + moduleId.ToString() + roleId.ToString()) as IEnumerable<SubModule>;

            if (subModuleList == null)
            {
                subModuleList = this.subModuleRepository.GetMany(sm => sm.ModuleId == moduleId && sm.IsActive == true);
                foreach (var sm in subModuleList.ToList())
                {
                    var smiList = sm.SubModuleItems.ToList();
                    foreach (var smi in smiList)
                    {
                        //exclude Sub module item if all CRUD off or null in RoleSubModuleItems
                        var rsmiForCRUD = smi.RoleSubModuleItems
                            .Where(rs => rs.RoleId == roleId &&
                                         (rs.CreateOperation == true || rs.ReadOperation == true ||
                                          rs.UpdateOperation == true || rs.DeleteOperation == true)).FirstOrDefault();
                        if (rsmiForCRUD == null)
                        {
                            if (smi.IsBaseItem != null && smi.IsBaseItem.Value == false)
                            {
                                var tmpsmi = smiList.Where(tsmi => tsmi.Id == smi.BaseItemId).FirstOrDefault();
                                if (tmpsmi != null)
                                {
                                    rsmiForCRUD = tmpsmi.RoleSubModuleItems
                                        .Where(rs => rs.RoleId == roleId &&
                                                     (rs.CreateOperation == true || rs.ReadOperation == true ||
                                                      rs.UpdateOperation == true || rs.DeleteOperation == true))
                                        .FirstOrDefault();
                                    if (rsmiForCRUD == null)
                                    {
                                        sm.SubModuleItems.Remove(smi);
                                    }
                                }
                                else
                                {
                                    var sbi = this.subModuleItemRepository.Get(ch => ch.BaseItemId == smi.BaseItemId);
                                    if (sbi != null)
                                    {
                                        var sbiForCRUD = sbi.RoleSubModuleItems
                                            .Where(rs => rs.RoleId == roleId &&
                                                         (rs.CreateOperation == true || rs.ReadOperation == true ||
                                                          rs.UpdateOperation == true || rs.DeleteOperation == true))
                                            .FirstOrDefault();
                                        if (sbiForCRUD == null)
                                        {
                                            sm.SubModuleItems.Remove(smi);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                sm.SubModuleItems.Remove(smi);
                            }
                        }

                    }
                }
            }
            if (subModuleList.Any())
            {
                cacheProvider.Set("submodule" + moduleId.ToString() + roleId.ToString(), subModuleList, 30);
            }

            return subModuleList;
        }
        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
