using Remit.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace Remit.WebAPI.Controllers
{
    public class BankController : ApiController
    {
        public readonly IBankService bankService;
      

        public BankController(IBankService bankService)
        {
            this.bankService = bankService;
           
        }

        public JsonResult<List<BankViewModel>> GetBankList()
        {
            var bankListObj = this.bankService.GetAllBank();
            List<BankViewModel> bankVMList = new List<BankViewModel>();

            foreach (var bank in bankListObj)
            {
                BankViewModel bankTemp = new BankViewModel();
                bankTemp.Id = bank.Id;
                bankTemp.Name = bank.Name;
                
                bankVMList.Add(bankTemp);
            }
            return Json(bankVMList);
        }

        public JsonResult<BankViewModel> GetBank(int id)
        {
            var bank = this.bankService.GetBank(id);
            BankViewModel bankTemp = new BankViewModel();
            bankTemp.Id = bank.Id;
            bankTemp.Name = bank.Name;
            return Json(bankTemp);
        }
    }
    public class BankViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
      

    }
}
