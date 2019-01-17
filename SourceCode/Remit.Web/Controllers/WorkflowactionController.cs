using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Helpers;


namespace Remit.Web.Controllers
{
    public class WorkflowactionController : Controller
    {
        public readonly IWorkflowactionService workflowactionService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public WorkflowactionController(IWorkflowactionService workflowactionService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.workflowactionService = workflowactionService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:workflowaction" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /Workflowaction/
        public ActionResult Index()
        {
            const string url = "/Workflowaction/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set("permission:workflowaction" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("Workflowaction");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult Workflowaction()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateWorkflowaction(Workflowaction workflowaction)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = workflowaction.Id == 0 ? true : false;
            const string url = "/Workflowaction/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew)
            {

                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(workflowaction))
                    {

                        if (this.workflowactionService.CreateWorkflowaction(workflowaction))
                        {
                            isSuccess = true;
                            message = "Workflowaction saved successfully!";
                        }
                        else
                        {
                            message = "Workflowaction could not saved!";
                        }


                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same workflowaction name found!";
                    }
                }
                else
                {
                    message = Resources.ResourceCommon.MsgNoPermissionToCreate;
                }


            }
            else
            {
                if (permission.UpdateOperation == true)
                {
                    if (this.workflowactionService.UpdateWorkflowaction(workflowaction))
                    {
                        isSuccess = true;
                        message = "Workflowaction updated successfully!";
                    }
                    else
                    {
                        message = "Workflowaction could not updated!";
                    }
                }
                else
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
        private bool CheckIsExist(Model.Models.Workflowaction workflowaction)
        {
            return this.workflowactionService.CheckIsExist(workflowaction);
        }
        [HttpPost]
        public JsonResult DeleteWorkflowaction(Workflowaction workflowaction)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/Workflowaction/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.workflowactionService.DeleteWorkflowaction(workflowaction.Id);
                if (isSuccess)
                {
                    message = "Workflowaction deleted successfully!";

                }
                else
                {
                    message = "Workflowaction can't be deleted!";
                }
            }
            else
            {
                message = Resources.ResourceCommon.MsgNoPermissionToDelete;
            }


            return Json(new
            {
                isSuccess = isSuccess,
                message = message
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetWorkflowactionList()
        {
            var workflowactionListObj = this.workflowactionService.GetAllWorkflowaction();
            List<WorkflowactionViewModel> workflowactionVMList = new List<WorkflowactionViewModel>();

            foreach (var workflowaction in workflowactionListObj)
            {
                WorkflowactionViewModel workflowactionTemp = new WorkflowactionViewModel();
                workflowactionTemp.Id = workflowaction.Id;
                workflowactionTemp.Name = workflowaction.Name;

                workflowactionVMList.Add(workflowactionTemp);
            }
            return Json(workflowactionVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetWorkflowaction(int id)
        {
            var workflowaction = this.workflowactionService.GetWorkflowaction(id);
            return Json(workflowaction);
        }
    }

    public class WorkflowactionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
}