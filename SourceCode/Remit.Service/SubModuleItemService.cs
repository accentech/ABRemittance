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
    public interface ISubModuleItemService
    {

        bool CreateSubModuleItem(SubModuleItem subModuleItem);
        bool UpdateSubModuleItem(SubModuleItem subModuleItem);
        bool DeleteSubModuleItem(int id);
        SubModuleItem GetSubModuleItem(int id);
       
        IEnumerable<SubModuleItem> GetAllSubModuleItem();
        IEnumerable<SubModuleItem> GetAllBaseSubModuleItem();
        void SaveRecord();
        bool CheckIsExist(SubModuleItem subModuleItem);
        SubModuleItem GetSubModuleItemUrl(string url);
    }
    public class SubModuleItemService : ISubModuleItemService
    {
        public SubModuleItemService()
        {

        }
        private readonly ISubModuleItemRepository subModuleItemRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(SubModuleItemService));

        public SubModuleItemService(ISubModuleItemRepository subModuleItemRepository, IUnitOfWork unitOfWork)
        {
            this.subModuleItemRepository = subModuleItemRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CheckIsExist(SubModuleItem subModuleItem)
        {
            return subModuleItemRepository.Get(chk => chk.Name == subModuleItem.Name && chk.SubModuleId==subModuleItem.Id) == null ? false : true;
        }

        public bool CreateSubModuleItem(SubModuleItem subModuleItem)
        {
            bool isSuccess = true;
            try
            {
                subModuleItemRepository.Add(subModuleItem);
                this.SaveRecord();
                ServiceUtil<SubModuleItem>.WriteActionLog(subModuleItem.Id, ENUMOperation.CREATE, subModuleItem);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating SubModuleItem", ex);
            }
            return isSuccess;
        }

        public bool UpdateSubModuleItem(SubModuleItem subModuleItem)
        {
            bool isSuccess = true;
            try
            {
                subModuleItemRepository.Update(subModuleItem);
                this.SaveRecord();
                ServiceUtil<SubModuleItem>.WriteActionLog(subModuleItem.Id, ENUMOperation.UPDATE, subModuleItem);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating SubModuleItem", ex);
            }
            return isSuccess;
        }

        public bool DeleteSubModuleItem(int id)
        {
            bool isSuccess = true;
            var subModuleItem = subModuleItemRepository.GetById(id);
            try
            {
                subModuleItemRepository.Delete(subModuleItem);
                SaveRecord();
                ServiceUtil<SubModuleItem>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting SubModuleItem", ex);
            }
            return isSuccess;
        }

        public SubModuleItem GetSubModuleItem(int id)
        {
            return subModuleItemRepository.GetById(id);
        }

        
        public IEnumerable<SubModuleItem> GetAllSubModuleItem()
        {
            return subModuleItemRepository.GetAll();
        }

        public IEnumerable<SubModuleItem> GetAllBaseSubModuleItem()
        {
            return subModuleItemRepository.GetMany(sm => sm.IsBaseItem == true && sm.IsActive == true).OrderBy(sm1 => sm1.Name);
        }


        public SubModuleItem GetSubModuleItemUrl(string url)
        {
            var getStation = subModuleItemRepository.Get(st => st.UrlPath == url);
            return getStation;

        }
        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
