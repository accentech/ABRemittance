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
    public interface IEmployeeService
    {

        bool CreateEmployee(Employee employee);
        bool UpdateEmployee(Employee employee);
        bool DeleteEmployee(int id);
        Employee GetEmployee(int id);

        IEnumerable<Employee> GetAllEmployee();
        IEnumerable<Employee> GetAllEmployeeByDepartmentAndDesignation(int? departmentId, int? designationId);
        void SaveRecord();

        bool CheckIsExist(Employee employee);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository employeeRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(EmployeeService));

        public EmployeeService()
        {
        }
                
        public EmployeeService(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
        {
            this.employeeRepository = employeeRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CheckIsExist(Employee employee)
        {
            return employeeRepository.Get(chk => chk.Code == employee.Code) == null ? false : true;
        }

        public bool CreateEmployee(Employee employee)
        {
            bool isSuccess = true;
            try
            {
                employeeRepository.Add(employee);                
                this.SaveRecord();
                ServiceUtil<Employee>.WriteActionLog(employee.Id, ENUMOperation.CREATE, employee);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating Employee", ex);
            }
            return isSuccess;
        }

        public bool UpdateEmployee(Employee employee)
        {
            bool isSuccess = true;
            try
            {
                employeeRepository.Update(employee);
                this.SaveRecord();
                ServiceUtil<Employee>.WriteActionLog(employee.Id, ENUMOperation.UPDATE, employee);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating Employee", ex);
            }
            return isSuccess;
        }

        public bool DeleteEmployee(int id)
        {
            bool isSuccess = true;
            var employee = employeeRepository.GetById(id);
            try
            {
                employeeRepository.Delete(employee);
                SaveRecord();
                ServiceUtil<Employee>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch(Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting Employee", ex);
            }
            return isSuccess;
        }

        public Employee GetEmployee(int id)
        {
            return employeeRepository.GetById(id);
        }
               
        public IEnumerable<Employee> GetAllEmployee()
        {
            return employeeRepository.GetAll();
        }

        

        public IEnumerable<Employee> GetAllEmployeeByDepartmentAndDesignation(int? departmentId, int? designationId)
        {
            //return  this.employeeRepository.GetMany(emp => emp.EmploymentHistories.OrderByDescending(eehh => eehh.DateFrom).Any(eh => eh.DepartmentId == designationId && eh.DesignationId == designationId));
            if(departmentId != null && designationId != null)
            {
                var employeeObj = this.employeeRepository.GetAll();// && DesignationId == designationId)); //current designation date to is null
                return employeeObj;
            }
            //else if (departmentId != null && designationId == null)
            //{
            //    var employeeObj = this.employeeRepository.GetMany();     //current department/designation is null
            //    return employeeObj;
            //}
            else
            {
                return this.employeeRepository.GetAll();
            }
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
