using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Model.Models;
using Remit.Data.Repository;
using Remit.Data.Infrastructure;
using Remit.LoggerService;
using Remit.Core.Common;

namespace Remit.Service
{
    public interface IUserService
    {
        bool CreateUser(BusinessUser user);
        bool UpdateUser(BusinessUser user);
        bool DeleteUser(int id);
        BusinessUser GetUser(int id);
        BusinessUser GetUserByLoginNameAndPassword(BusinessUser user);
        BusinessUser GetUserByEmail(string email);

        BusinessUser AuthenticateUser(BusinessUser user);
        bool ForgetPwdExpired(BusinessUser user);
        IEnumerable<BusinessUser> GetAllUser();

        void SaveRecord();
    }
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly IEmployeeRepository employeeRepository;
        private readonly IUnitOfWork unitOfWork;
        private double pwdvalidMinute = 20;
        private readonly LoggingService logger = new LoggingService(typeof(UserService));

        public UserService(IUserRepository userRepository, IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
        {
            this.userRepository = userRepository;
            this.employeeRepository = employeeRepository;
            this.unitOfWork = unitOfWork;
        }

        public bool CreateUser(BusinessUser user)
        {
            bool isSuccess = true;
            try
            {
                userRepository.Add(user);
                this.SaveRecord();
                ServiceUtil<BusinessUser>.WriteActionLog(user.Id, ENUMOperation.CREATE, user);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating User", ex);
            }
            return isSuccess;
        }

        public bool UpdateUser(BusinessUser user)
        {
            bool isSuccess = true;
            try
            {
                userRepository.Update(user);
                this.SaveRecord();
                ServiceUtil<BusinessUser>.WriteActionLog(user.Id, ENUMOperation.UPDATE, user);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating User", ex);
            }
            return isSuccess;
        }

        public bool DeleteUser(int id)
        {
            bool isSuccess = true;
            try
            {
                BusinessUser user = GetUser(id);
                userRepository.Delete(user);
                SaveRecord();
                ServiceUtil<BusinessUser>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting User", ex);
            }
            return isSuccess;
        }

        public BusinessUser GetUser(int id)
        {
            return userRepository.GetById(id);
        }
        public BusinessUser GetUserByLoginNameAndPassword(BusinessUser user)
        {
            return userRepository.Get(u => u.LoginName == user.LoginName && u.Password == user.Password);
        }

        public BusinessUser GetUserByEmail(string email)
        {
            return userRepository.Get(u => u.EmployeeId == employeeRepository.Get(e=>e.Email==email).Id);
        }

        public BusinessUser AuthenticateUser(BusinessUser user)
        {
            BusinessUser getUserInfo = new BusinessUser();
            try
            {
                getUserInfo = userRepository.Get(u => u.LoginName.ToUpper() == user.LoginName.ToUpper() && u.Password == user.Password);
                if (getUserInfo != null)
                {
                    if (ForgetPwdExpired(getUserInfo))
                    {
                        getUserInfo = null;
                    }
                }
            }
            catch (Exception e)
            {
                getUserInfo = null;
                logger.Error("Error in authenticating user", e);
            }
            return getUserInfo;
        }

        public bool ForgetPwdExpired(BusinessUser user)
        {
            bool isExpired = false;
            if (user.PwdTimeStamp.HasValue)
            {
                DateTime pwdValidtime = user.PwdTimeStamp.Value.AddMinutes(pwdvalidMinute);
                if (DateTime.UtcNow > pwdValidtime)
                {
                    isExpired = true; // if current time <=pwd validation time pwd not expired
                }
            }
            return isExpired;
        }

        public IEnumerable<BusinessUser> GetAllUser()
        {
            return userRepository.GetAll();
        }

        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
