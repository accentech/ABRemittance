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
    public interface IModuleService
    {

        bool CreateModule(Module module);
        bool UpdateModule(Module module);
        bool DeleteModule(int id);
        Module GetModule(int id);

        IEnumerable<Module> GetAllModule();
        IEnumerable<Module> GetAllModuleByRoleId(int? roleId);
        void SaveRecord();
        bool CheckIsExist(Module module);
    }
    public class ModuleService : IModuleService
    {
        public ModuleService()
        {

        }
        private readonly IModuleRepository moduleRepository;
        private readonly ISubModuleItemRepository subModuleItemRepository;
        private readonly IRoleSubModuleItemRepository roleSubModuleItemRepository;
        private readonly IUnitOfWork unitOfWork;

        private readonly ICacheProvider cacheProvider = new DefaultCacheProvider();
        private readonly LoggingService logger = new LoggingService(typeof(ModuleService));

        public ModuleService(IModuleRepository moduleRepository, ISubModuleItemRepository subModuleItemRepository, IRoleSubModuleItemRepository roleSubModuleItemRepository, IUnitOfWork unitOfWork)
        {
            this.moduleRepository = moduleRepository;
            this.subModuleItemRepository = subModuleItemRepository;
            this.roleSubModuleItemRepository = roleSubModuleItemRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CheckIsExist(Module module)
        {
            return moduleRepository.Get(chk => chk.Name == module.Name) == null ? false : true;
        }

        public bool CreateModule(Module module)
        {
            bool isSuccess = true;
            try
            {
                moduleRepository.Add(module);
                this.SaveRecord();
                ServiceUtil<Module>.WriteActionLog(module.Id, ENUMOperation.CREATE, module);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Module", ex);
            }
            return isSuccess;
        }

        public bool UpdateModule(Module module)
        {
            bool isSuccess = true;
            try
            {
                moduleRepository.Update(module);
                this.SaveRecord();
                ServiceUtil<Module>.WriteActionLog(module.Id, ENUMOperation.UPDATE, module);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Module", ex);
            }
            return isSuccess;
        }

        public bool DeleteModule(int id)
        {
            bool isSuccess = true;
            var module = moduleRepository.GetById(id);
            try
            {
                moduleRepository.Delete(module);
                SaveRecord();
                ServiceUtil<Module>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Module", ex);
            }
            return isSuccess;
        }

        public Module GetModule(int id)
        {
            return moduleRepository.GetById(id);
        }


        public IEnumerable<Module> GetAllModule()
        {
            IEnumerable<Module> modules = cacheProvider.Get("module") as IEnumerable<Module>;


            if (modules == null)
            {
                modules = moduleRepository.GetAll();
                if (modules.Any())
                {
                    cacheProvider.Set("module", modules, 240);
                }
            }
            return moduleRepository.GetAll();
        }

        public IEnumerable<Module> GetAllModuleByRoleId(int? roleId)
        {

            IEnumerable<Module> moduleList = cacheProvider.Get("module" + roleId.ToString()) as IEnumerable<Module>;
            List<Module> moduleListObj = new List<Module>();


            if (moduleList == null)
            {
                moduleList = this.moduleRepository.GetAll();
                if (moduleList != null)
                    moduleList = moduleList.OrderBy(m => m.Ordering);
                foreach (var m in moduleList.ToList())
                {
                    foreach (var sm in m.SubModules.ToList())
                    {
                        foreach (var smi in sm.SubModuleItems.ToList())
                        {
                            //exclude Sub module item if all CRUD off or null in RoleSubModuleItems
                            var rsmiForCRUD = smi.RoleSubModuleItems.Where(rs => rs.RoleId == roleId && (rs.CreateOperation == true || rs.ReadOperation == true || rs.UpdateOperation == true || rs.DeleteOperation == true)).FirstOrDefault();
                            if (rsmiForCRUD == null)
                            {
                                sm.SubModuleItems.Remove(smi);
                            }
                        }

                        if (sm.SubModuleItems.Count() == 0)
                        {
                            m.SubModules.Remove(sm);
                        }
                    }
                    if (m.SubModules.Count() == 0)
                    {
                        moduleList.ToList().Remove(m); //modue have to hide ((from UI)) when SubModule count is 0, coz item remove not working here
                        continue;
                    }
                    Module aModule = new Module();
                    aModule.Id = m.Id;
                    aModule.ImageName = m.ImageName;
                    aModule.Name = m.Name;
                    aModule.Ordering = m.Ordering;

                    moduleListObj.Add(aModule);
                }
                if (moduleListObj.Any())
                {
                    cacheProvider.Set("module" + roleId.ToString(), moduleListObj, 30);
                }
                return moduleListObj;
            }
            return moduleList;
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
