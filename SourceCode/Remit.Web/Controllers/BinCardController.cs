
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using Remit.CachingService;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class BinCardController : Controller
    {
        public readonly ICountryService countryService;
        public readonly IBinCardService binCardService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:binCard" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /BinCard/
        public ActionResult Index()
        {
            var url = Request.RawUrl;

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("BinCard");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public BinCardController(IBinCardService binCardService, ICountryService countryService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.binCardService = binCardService;
            this.countryService = countryService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }


        [HttpPost]
        public JsonResult CreateBinCard(BinCard binCard)
        {
            const string url = "/BinCard/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = binCard.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(binCard))
                    {
                        if (this.binCardService.CreateBinCard(binCard))
                        {
                            isSuccess = true;
                            message = "BinCard saved successfully!";
                        }
                        else
                        {
                            message = "BinCard could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same binCard name found!";
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
                    if (this.binCardService.UpdateBinCard(binCard))
                    {
                        isSuccess = true;
                        message = "BinCard updated successfully!";
                    }
                    else
                    {
                        message = "BinCard could not updated!";
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
        private bool CheckIsExist(Model.Models.BinCard binCard)
        {
            return this.binCardService.CheckIsExist(binCard);
        }


        [HttpPost]
        public JsonResult DeleteBinCard(BinCard binCard)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/SubModuel/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.binCardService.DeleteBinCard(binCard.Id);
                if (isSuccess)
                {
                    message = "BinCard deleted successfully!";
                }
                else
                {
                    message = "BinCard can't be deleted!";
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

        public JsonResult GetBinCardList()
        {
            var binCardListObj = this.binCardService.GetAllBinCard();
            List<BinCardViewModel> binCardVMList = new List<BinCardViewModel>();

            foreach (var binCard in binCardListObj)
            {
                BinCardViewModel binCardTemp = new BinCardViewModel();
                binCardTemp.Id = binCard.Id;
                binCardTemp.CardNo = binCard.CardNo;
                if (binCard.Warehouse != null)
                {
                    if (binCard.WarehouseId != null) binCardTemp.WarehouseId =(int) binCard.WarehouseId;
                    binCardTemp.WarhouseName = binCard.Warehouse.Name;
                }
                binCardVMList.Add(binCardTemp);
            }
            return Json(binCardVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBinCard(int id)
        {
            var binCard = this.binCardService.GetBinCard(id);
            return Json(binCard);
        }
    }

    public class BinCardViewModel
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public string CardNo { get; set; }
       
        public string WarhouseName { get; set; }

        public virtual Warehouse Warehouse { get; set; }


    }

}