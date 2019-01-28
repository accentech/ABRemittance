using Remit.ClientModel;
using Remit.Service;
using Remit.Model.Models;
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

        // GET api/bank
        public JsonResult<List<BankModel>> GetAll()
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
        
        // GET api/bank/5
        public JsonResult<BankModel> Get(int id)
        {
            var bank = this.bankService.GetBank(id);
            BankModel bankTemp = new BankModel();
            bankTemp.Id = bank.Id;
            bankTemp.Name = bank.Name;
            return Json(bankTemp);
        }

        // POST api/bank
        public string Post(Bank bank)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = bankService.GetBank(bank.Id);

            if (isNew == null)
            {
                if (!CheckIsExist(bank))
                {
                    if (this.bankService.CreateBank(bank))
                    {
                        isSuccess = true;
                        message = "Bank saved successfully!";
                    }
                    else
                    {
                        message = "Bank could not saved!";
                    }
                }
                else
                {
                    isSuccess = false;
                    message = "Can't save. Same bank name found!";
                }
            }
            else
            {
                if (this.bankService.UpdateBank(isNew))
                {
                    isSuccess = true;
                    message = "Bank updated successfully!";
                }
                else
                {
                    message = "Bank could not updated!";
                }
            }

            return message;
        }

        private bool CheckIsExist(Model.Models.Bank bank)
        {
            return this.bankService.CheckIsExist(bank);
        }

        // DELETE api/bank/5
        public string Delete(int id)
        {
            var isSuccess = true;
            var message = string.Empty;

            isSuccess = this.bankService.DeleteBank(id);
            if (isSuccess)
            {
                message = "Bank deleted successfully!";

            }
            else
            {
                message = "Bank can't be deleted!";
            }

            return message;
        }
    }
}
