using Remit.ClientModel;
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

        public JsonResult<List<BankModel>> GetBankList()
        {
            var bankListObj = this.bankService.GetAllBank();
            List<BankModel> bankVMList = new List<BankModel>();

            foreach (var bank in bankListObj)
            {
                BankModel bankTemp = new BankModel();
                bankTemp.Id = bank.Id;
                bankTemp.Name = bank.Name;
                
                bankVMList.Add(bankTemp);
            }
            return Json(bankVMList);
        }

        public JsonResult<BankModel> GetBank(int id)
        {
            var bank = this.bankService.GetBank(id);
            BankModel bankTemp = new BankModel();
            bankTemp.Id = bank.Id;
            bankTemp.Name = bank.Name;
            return Json(bankTemp);
        }
    }
    
}
