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
    public interface IDivisionService
    {
        bool CreateDivision(Division division);
        bool UpdateDivision(Division division);
        
        bool DeleteDivision(int id);
        Division GetDivision(int id);
        IEnumerable<Division> GetAllDivision();
        void SaveRecord();
       
        bool CheckIsExist(Division division);
        
    }

    public class DivisionService : IDivisionService
    {
        private readonly IDivisionRepository divisionRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(DivisionService));

        public DivisionService()
        {
        }
        public DivisionService(IDivisionRepository divisionRepository, IUnitOfWork unitOfWork)
        {

            this.divisionRepository = divisionRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(Division division)
        {
           
            return divisionRepository.Get(chk => chk.Name == division.Name) == null ? false : true;
        }

        public bool CreateDivision(Division division)
        {
            bool isSuccess = true;
            try
            {
                divisionRepository.Add(division);
                this.SaveRecord();
                ServiceUtil<Division>.WriteActionLog(division.Id, ENUMOperation.CREATE, division);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Division", ex);
            }
            return isSuccess;
        }

        public bool UpdateDivision(Division division)
        {
            bool isSuccess = true;
            try
            {
                divisionRepository.Update(division);
                this.SaveRecord();
                ServiceUtil<Division>.WriteActionLog(division.Id, ENUMOperation.UPDATE, division);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Division", ex);
            }
            return isSuccess;
        }

        public bool DeleteDivision(int id)
        {
            bool isSuccess = true;
            var division = divisionRepository.GetById(id);
            try
            {
                divisionRepository.Delete(division);
                SaveRecord();
                ServiceUtil<Division>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Division", ex);
            }
            return isSuccess;
        }

        public Division GetDivision(int id)
        {
            return divisionRepository.GetById(id);
        }

        public IEnumerable<Division> GetAllDivision()
        {
            return divisionRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
