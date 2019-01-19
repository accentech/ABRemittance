using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.ClientModel;
using Remit.Model.Models;
using Remit.Service;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
  public class EmployeeController : Controller
  {
    public readonly IEmployeeService employeeService;

    public readonly ISubModuleItemService subModuleItemService;
    public readonly IRoleSubModuleItemService roleSubModuleItemService;
    public readonly IWorkflowactionSettingService workflowactionSettingService;
    private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

    protected long timeZoneOffset = UserSession.GetTimeZoneOffset();
    public EmployeeController( IEmployeeService employeeService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService, IWorkflowactionSettingService workflowactionSettingService )
    {
      this.employeeService = employeeService;
      this.subModuleItemService = subModuleItemService;
      this.workflowactionSettingService = workflowactionSettingService;
     
      this.roleSubModuleItemService = roleSubModuleItemService;
    }

    string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
    string cacheKey = "permission:employee" + Helpers.UserSession.GetUserFromSession().RoleId;
    RoleSubModuleItem permission = null;

    // GET: /Employee/
    public ActionResult Index()
    {
        const string url = "/Employee/Index";
        permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                     roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

        if (permission != null)
        {
            if (permission.ReadOperation == true)
            {
                cacheProvider.Set(cacheKey, permission, 240);
                return View("Employee");
            }
            else
            {
                return View("~/Views/Shared/NoPermission.cshtml");
            }
        }

        return View("~/Views/Shared/NoPermission.cshtml");
    }

    // GET: /Employee/
    public ActionResult Operation()
    {
        ViewBag.Operation = 1;
        const string url = "/Employee/Operation";
        permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                     roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

        if (permission != null)
        {
            if (permission.ReadOperation == true)
            {
                cacheProvider.Set(cacheKey, permission, 240);
                return View("Employee");
            }else{
                return View("~/Views/Shared/NoPermission.cshtml");
            }
        }

        return View("~/Views/Shared/NoPermission.cshtml");
    }

    [HttpPost]
    public JsonResult CreateEmployee(Employee employee, string fileName)
    {
      var isSuccess = false;
      var message = string.Empty;
      var isNew = employee.Id == 0 ? true : false;
      const string url = "/Employee/Index";
      permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                   roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

      if ( isNew )
      {
        if ( permission.CreateOperation == true )
        {
          if ( !CheckIsExist(employee) )
          {
            //foreach ( var emp in employee.EmploymentHistories )
            //{
            //  if ( emp.DateFrom != null )
            //    emp.DateFrom = emp.DateFrom.Value.ToUniversalTime();
            //  if ( emp.DateTo != null )
            //    emp.DateTo = emp.DateTo.Value.ToUniversalTime();
            //  emp.EmployeeId = employee.Id;
            //}

              if (fileName != null)
              {
                  fileName = fileName.Replace(" ", "_");
                  employee.PhotoPath = employee.Code + '_' + fileName; 
              }

            if ( this.employeeService.CreateEmployee(employee) )
            {

              isSuccess = true;
              message = "Employee saved successfully!";
            } else
            {
              message = "Employee could not saved!";
            }
          } else
          {
            isSuccess = false;
            message = "Can't save. Same employee name found!";
          }
        } else
        {
          message = Resources.ResourceCommon.MsgNoPermissionToCreate;
        }
      } else
      {
        if ( permission.UpdateOperation == true )
        {
          var employeeObj = this.employeeService.GetEmployee(employee.Id);
          if ( employeeObj != null )
          {
            //if ( employeeObj.EmploymentHistories != null )
            //{
            //  foreach ( var empHistory in employeeObj.EmploymentHistories.ToList() )
            //  {
            //    this.employmentHistoryService.DeleteEmploymentHistory(empHistory.Id);
            //  }
            //}
          }

          

          var employeeObjAttach = this.employeeService.GetEmployee(employee.Id);
          if ( employeeObjAttach != null )
          {
            employeeObjAttach.Code = employee.Code;
            employeeObjAttach.FullName = employee.FullName;
            employeeObjAttach.PresentAddress = employee.PresentAddress;
            employeeObjAttach.PermanentAddress = employee.PermanentAddress;
            employeeObjAttach.FatherName = employee.FatherName;
            employeeObjAttach.MotherName = employee.MotherName;
            employeeObjAttach.NationalId = employee.NationalId;
            employeeObjAttach.PassportNo = employee.PassportNo;

              //if (employeeObjAttach.PhotoPath == employee.PhotoPath)
              //{
              //    employeeObjAttach.PhotoPath = employee.PhotoPath;
              //}
              if (fileName != "")
              {
                  fileName = fileName.Replace(" ", "_");
                  employeeObjAttach.PhotoPath = employee.Code + '_' + fileName;
              }

             
              
             
           
            employeeObjAttach.Email = employee.Email;
            employeeObjAttach.OfficePhone = employee.OfficePhone;
            employeeObjAttach.OfficeMobile = employee.OfficeMobile;
            employeeObjAttach.ResidentPhone = employee.ResidentPhone;
            employeeObjAttach.ResidentMobile = employee.ResidentMobile;
            employeeObjAttach.BloodGroup = employee.BloodGroup;

            if ( this.employeeService.UpdateEmployee(employeeObjAttach) )
            {
              isSuccess = true;
              message = "Employee updated successfully!";
            } else
            {
              message = "Employee could not updated!";
            }
          }
        } else
        {
          message = Resources.ResourceCommon.MsgNoPermissionToUpdate;
        }

      }

      return Json(new
      {
        isSuccess = isSuccess,
        message = message,
      }, JsonRequestBehavior.AllowGet);
    }
    private bool CheckIsExist( Model.Models.Employee employee )
    {
      return this.employeeService.CheckIsExist(employee);
    }
    [HttpPost]
    public JsonResult DeleteEmployee( Employee employee )
    {
      var isSuccess = true;
      var message = string.Empty;
      const string url = "/Employee/Index";
      permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                          Helpers.UserSession.GetUserFromSession().RoleId);

      if ( permission.DeleteOperation == true )
      {
        var employeeObj = this.employeeService.GetEmployee(employee.Id);
        if ( employeeObj != null )
        {
          
        }

        isSuccess = this.employeeService.DeleteEmployee(employee.Id);
        if ( isSuccess )
        {
          message = "Employee deleted successfully!";

        } else
        {
          message = "Employee can't be deleted!";
        }
      } else
      {
        message = Resources.ResourceCommon.MsgNoPermissionToDelete;
      }


      return Json(new
      {
        isSuccess = isSuccess,
        message = message
      }, JsonRequestBehavior.AllowGet);
    }


//upload file..............

      //public void UpoladFile(string name, string code, HttpPostedFileBase file)
      //{
      //    var length = Request.ContentLength;
      //    var bytes = new byte[length];
      //    Request.InputStream.Read(bytes, 0, length);

      //    string str = file.FileName;
      //    string ext = str.Substring(0, str.LastIndexOf(".") + 1).TrimEnd('.');

      //    if (System.IO.File.Exists(Server.MapPath("~/Files/EmployeeImage/" + code + '_' + ext + Path.GetExtension(file.FileName))))
      //    {
      //        System.IO.File.Delete(Server.MapPath("~/Files/EmployeeImage/" + code + '_' + ext + Path.GetExtension(file.FileName)));
      //    }
      //    var saveToFileLoc = Server.MapPath("~/Files/EmployeeImage/" + code + '_' + ext + Path.GetExtension(file.FileName));

      //    var fileStream = new FileStream(saveToFileLoc, FileMode.Create, FileAccess.ReadWrite);
      //    fileStream.Write(bytes, 0, length);
      //    fileStream.Close();
      //}


    public void UpoladFile(string name, string code, HttpPostedFileBase file)
      {
          if (file != null)
          {
              var mainFolder = Server.MapPath("~/Files");
              if (!Directory.Exists(mainFolder))
              {
                  Directory.CreateDirectory(mainFolder);
              }


              var folder = Server.MapPath("~/Files/EmployeeImage");
              if (!Directory.Exists(folder))
              {
                  Directory.CreateDirectory(folder);
              }


              string str = file.FileName;
              string ext = str.Substring(0, str.LastIndexOf(".") + 1).TrimEnd('.');
              if (ext != String.Empty)
              {
                  ext = ext.Replace(" ", "_");
              }

              if (System.IO.File.Exists(
                  Server.MapPath("~/Files/EmployeeImage/" + code + '_' + ext + Path.GetExtension(file.FileName))))
              {
                  System.IO.File.Delete(
                      Server.MapPath("~/Files/EmployeeImage/" + code + '_' + ext + Path.GetExtension(file.FileName)));
              }

              var saveToFileLoc = Server.MapPath("~/Files/EmployeeImage/" + code + '_' + ext + Path.GetExtension(file.FileName));
              try
              {
                  file.SaveAs(saveToFileLoc);
              }
              catch (Exception e)
              {
                  Console.WriteLine("File Save Error: " + e);
              }
          }


      }




    public JsonResult GetEmployeeList()
    {
        var employeeListObj = this.employeeService.GetAllEmployee();
        List<EmployeeModel> employeeVMList = new List<EmployeeModel>();

        foreach (var employee in employeeListObj)
        {
            EmployeeModel employeeTemp = new EmployeeModel();
            employeeTemp.Id = employee.Id;
            employeeTemp.Code = employee.Code;
            employeeTemp.FullName = employee.FullName;
            employeeTemp.PresentAddress = employee.PresentAddress;
            employeeTemp.PermanentAddress = employee.PermanentAddress;
            employeeTemp.FatherName = employee.FatherName;
            employeeTemp.MotherName = employee.MotherName;
            employeeTemp.Email = employee.Email;
            employeeTemp.NationalId = employee.NationalId;
            employeeTemp.OfficeMobile = employee.OfficeMobile;
            employeeTemp.OfficePhone = employee.OfficePhone;
            employeeTemp.PassportNo = employee.PassportNo;
            employeeTemp.PhotoPath = employee.PhotoPath;
            employeeTemp.ResidentMobile = employee.ResidentMobile;
            employeeTemp.ResidentPhone = employee.ResidentPhone;
            employeeTemp.BloodGroup = employee.BloodGroup;
            employeeTemp.AuthenticationCode = employee.AuthenticationCode;
            string LatestDepartmentName = "";
            string LatestDesignationName = "";
           
            employeeTemp.LatestDepartmentName = LatestDepartmentName;
            employeeTemp.LatestDesignationName =LatestDesignationName;

            if (employeeTemp.Id != 1)
                employeeVMList.Add(employeeTemp);
            else if (UserSession.IsAdmin())
                employeeVMList.Add(employeeTemp);
        }
        return Json(employeeVMList, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetEmployeeListOperation()
    {
        var employeeListObj = this.employeeService.GetAllEmployeeByConfig();
        List<EmployeeModel> employeeVMList = new List<EmployeeModel>();

        foreach (var employee in employeeListObj)
        {
            EmployeeModel employeeTemp = new EmployeeModel();
            employeeTemp.Id = employee.Id;
            employeeTemp.Code = employee.Code;
            employeeTemp.FullName = employee.FullName;
            employeeTemp.PresentAddress = employee.PresentAddress;
            employeeTemp.PermanentAddress = employee.PermanentAddress;
            employeeTemp.FatherName = employee.FatherName;
            employeeTemp.MotherName = employee.MotherName;
            employeeTemp.Email = employee.Email;
            employeeTemp.NationalId = employee.NationalId;
            employeeTemp.OfficeMobile = employee.OfficeMobile;
            employeeTemp.OfficePhone = employee.OfficePhone;
            employeeTemp.PassportNo = employee.PassportNo;
            employeeTemp.PhotoPath = employee.PhotoPath;
            employeeTemp.ResidentMobile = employee.ResidentMobile;
            employeeTemp.ResidentPhone = employee.ResidentPhone;
            employeeTemp.BloodGroup = employee.BloodGroup;
            employeeTemp.AuthenticationCode = employee.AuthenticationCode;


           

            if (employeeTemp.Id != 1)
                employeeVMList.Add(employeeTemp);
            else if (UserSession.IsAdmin())
                employeeVMList.Add(employeeTemp);
        }
        return Json(employeeVMList, JsonRequestBehavior.AllowGet);
    }


      public JsonResult GetEmployeeDep(int id)
      {
          var depName = "";
          var msg = true;
          var employee = this.employeeService.GetEmployee(id);
        

          return Json(new{depName = depName, msg = msg}, JsonRequestBehavior.AllowGet);
      }
    
    public JsonResult GetEmployee( int id )
    {
      var employee = this.employeeService.GetEmployee(id);
       
      return Json(employee);
    }

    [HttpPost]
    public JsonResult GetAllAvaiableUserForASubModuleItemFilterByDeptDesignation( int workflowactionId, int subModuleItemId, int? departmentId, int? designationId )
    {
      var allEmployeeList = this.employeeService.GetAllEmployeeByDepartmentAndDesignation(departmentId, designationId);

      var allUserSubModuleItemPreferenceList = this.workflowactionSettingService.GetWorkflowactionSettingBySubModuleItemId(subModuleItemId, workflowactionId);

      List<EmployeeModel> employeeList = new List<EmployeeModel>();

      foreach ( var user in allEmployeeList )
      {
        var itemExist = false;
        foreach ( var medicinePref in allUserSubModuleItemPreferenceList )
        {
          if ( user.Id == medicinePref.EmployeeId )
          {
            itemExist = true;
            break;
          }
        }
        if ( itemExist != true )
        {
          EmployeeModel employeeModelTemp = new EmployeeModel();
          employeeModelTemp.Id = user.Id;
          //employeeModelTemp.RoleId = user.RoleId;
          //if (user.EmployeeId != null)
          employeeModelTemp.FullName = user.FullName;

          if ( !employeeList.Contains(employeeModelTemp) )
          {
            employeeList.Add(employeeModelTemp);
          }
        }
      }

      //..........................For Selected Users.........................................//

      List<EmployeeModel> employeePreferanceVMList = new List<EmployeeModel>();

      foreach ( var userPreferance in allUserSubModuleItemPreferenceList )
      {
        EmployeeModel empPreferanceTemp = new EmployeeModel();
        if ( userPreferance.EmployeeId != null )
          empPreferanceTemp.Id = (int)userPreferance.EmployeeId;

        if ( userPreferance.EmployeeId != null )
        {
          empPreferanceTemp.FullName = userPreferance.Employee.FullName;
          //userPreferanceTemp.RoleId = userPreferance.Employee.RoleId;
        }

        employeePreferanceVMList.Add(empPreferanceTemp);
      }

      return Json(new
      {
        availableEmployees = employeeList,
        selectedEmployees = employeePreferanceVMList
      }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetAllAvaiableUserForASubModuleItemFilterByDeptDesignationForNotification( int subModuleItemId, int? departmentId, int? designationId )
    {
      var allEmployeeList = this.employeeService.GetAllEmployeeByDepartmentAndDesignation(departmentId, designationId);

      List<EmployeeModel> employeeList = new List<EmployeeModel>();

      foreach ( var emp in allEmployeeList )
      {
        var itemExist = false;
        if ( itemExist != true )
        {
          EmployeeModel employeeModelTemp = new EmployeeModel();
          employeeModelTemp.Id = emp.Id;
          //employeeModelTemp.RoleId = user.RoleId;
          employeeModelTemp.FullName = emp.FullName;

          if ( !employeeList.Contains(employeeModelTemp) )
          {
            employeeList.Add(employeeModelTemp);
          }
        }
      }

      //..........................For Selected Users.........................................//

      List<EmployeeModel> employeePreferanceVMList = new List<EmployeeModel>();

      return Json(new
      {
        availableEmployees = employeeList,
        selectedEmployees = employeePreferanceVMList
      }, JsonRequestBehavior.AllowGet);
    }
  }

 


}