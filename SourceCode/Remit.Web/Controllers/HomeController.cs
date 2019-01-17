using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.Model.Models;
using Remit.Service;
//using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        
      public ActionResult Index()
        {
            //UserSession.SetUserProfileFromSession();
            //UserSession.SetTimeZoneOffset(timeZoneOffset);
            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Description About AccenTech Limited.";

            return View();
        }
	}
}