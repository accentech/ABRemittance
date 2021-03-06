﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remit.Core.Common;
using Remit.Data.Infrastructure;
using Remit.Data.Repository;
using Remit.LoggerService;
using Remit.Model.Models;
using Remit.Service;

namespace Tiles.Service
{
    public interface IAutoGeneratedNoService
    {
        bool CreateAutoGeneratedNo(AutoGeneratedNo autoGeneratedNo);
        bool UpdateAutoGeneratedNo(AutoGeneratedNo autoGeneratedNo);

        bool DeleteAutoGeneratedNo(int id);
        AutoGeneratedNo GetAutoGeneratedNo(int id);
        IEnumerable<AutoGeneratedNo> GetAllAutoGeneratedNo();
        bool UpdateNo(string type);
        string GenerateNo(string type);
        void SaveRecord();

        bool CheckIsExist(AutoGeneratedNo autoGeneratedNo);

    }

    public class AutoGeneratedNoService : IAutoGeneratedNoService
    {
        private readonly IAutoGeneratedNoRepository autoGeneratedNoRepository;

        private readonly IUnitOfWork unitOfWork;
        private readonly LoggingService logger = new LoggingService(typeof(AutoGeneratedNoService));

        public AutoGeneratedNoService()
        {
        }
        public AutoGeneratedNoService(IAutoGeneratedNoRepository autoGeneratedNoRepository, IUnitOfWork unitOfWork)
        {

            this.autoGeneratedNoRepository = autoGeneratedNoRepository;
            this.unitOfWork = unitOfWork;
        }
        public bool CheckIsExist(AutoGeneratedNo autoGeneratedNo)
        {
           // return false;
            return autoGeneratedNoRepository.Get(chk => chk.Type == autoGeneratedNo.Type) == null ? false : true;
        }

        public bool CreateAutoGeneratedNo(AutoGeneratedNo autoGeneratedNo)
        {
            bool isSuccess = true;
            try
            {
                autoGeneratedNoRepository.Add(autoGeneratedNo);
                this.SaveRecord();
                ServiceUtil<AutoGeneratedNo>.WriteActionLog(autoGeneratedNo.Id, ENUMOperation.CREATE, autoGeneratedNo);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in creating AutoGeneratedNo", ex);
            }
            return isSuccess;
        }

        public bool UpdateAutoGeneratedNo(AutoGeneratedNo autoGeneratedNo)
        {
            bool isSuccess = true;
            try
            {
                autoGeneratedNoRepository.Update(autoGeneratedNo);
                this.SaveRecord();
                ServiceUtil<AutoGeneratedNo>.WriteActionLog(autoGeneratedNo.Id, ENUMOperation.UPDATE, autoGeneratedNo);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in updating AutoGeneratedNo", ex);
            }
            return isSuccess;
        }

        public bool DeleteAutoGeneratedNo(int id)
        {
            bool isSuccess = true;
            var autoGeneratedNo = autoGeneratedNoRepository.GetById(id);
            try
            {
                autoGeneratedNoRepository.Delete(autoGeneratedNo);
                SaveRecord();
                ServiceUtil<AutoGeneratedNo>.WriteActionLog(id, ENUMOperation.DELETE);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error("Error in deleting AutoGeneratedNo", ex);
            }
            return isSuccess;
        }

        public AutoGeneratedNo GetAutoGeneratedNo(int id)
        {
            return autoGeneratedNoRepository.GetById(id);
        }

       

        public IEnumerable<AutoGeneratedNo> GetAllAutoGeneratedNo()
        {
            return autoGeneratedNoRepository.GetAll();
        }

        public bool UpdateNo(string type)
        {
            var isSuccess = true;
            var chekExist = autoGeneratedNoRepository.Get(ato => ato.Type == type);
            if (chekExist != null)
            {
                chekExist.LastIndex = chekExist.LastIndex + 1;
                chekExist.LastIndexDate = DateTime.UtcNow;
                try
                {
                    autoGeneratedNoRepository.Update(chekExist);
                    this.SaveRecord();
                    ServiceUtil<AutoGeneratedNo>.WriteActionLog(chekExist.Id, ENUMOperation.UPDATE, chekExist);
                }
                catch { isSuccess = false; }
            }
            return isSuccess;
        }

        public string GenerateNo(string type)
        {
            var generatedNum = string.Empty;
            var chekExist = autoGeneratedNoRepository.Get(ato => ato.Type == type);
            if (chekExist != null)
            {
                //const string FMT = "O";
                //DateTime now1 = DateTime.UtcNow;
                //string strDate = now1.ToString(FMT);

                int length = chekExist.Format != null ? chekExist.Format.Length : 4;
                //var character = chekExist.Format != null ? chekExist.Format[0] : '0';
                string sep1 = string.Empty;
                string sep2 = string.Empty;
                char sep11 = chekExist.Separator1 != null ? chekExist.Separator1[0] : '0';
                sep1 = sep11 != '0' ?Convert.ToString(sep11) : string.Empty;
                
                char sep22 = chekExist.Separator2 != null ? chekExist.Separator2[0] : '0';
                sep2 = sep22 != '0' ? Convert.ToString(sep22) : string.Empty;
                
                //generatedNum = chekExist.Prefix + "_" + (chekExist.LastIndex + 1).ToString().PadLeft(4, '0');

                if (chekExist.RefreshFrequency != null)
                {
                    if (chekExist.RefreshFrequency.ToUpper() == "MONTHLY")
                    {
                        if (chekExist.LastIndexDate != null && (chekExist.LastIndexDate.Value.Year != DateTime.UtcNow.Year ||
                                                                chekExist.LastIndexDate.Value.Month != DateTime.UtcNow.Month))
                        { chekExist.LastIndex = 0; }
                    }
                    else if (chekExist.RefreshFrequency.ToUpper() == "YEARLY")
                    {
                        if (chekExist.LastIndexDate != null &&
                            chekExist.LastIndexDate.Value.Year != DateTime.UtcNow.Year)
                        { chekExist.LastIndex = 0; }
                    }
                    else if (chekExist.RefreshFrequency.ToUpper() == "QUARTERLY")
                    {

                    }
                    else if (chekExist.RefreshFrequency.ToUpper() == "DAILY")
                    {

                    }
                    else { }
                }

                //if()



                if (chekExist.PeriodFormat != null)
                {
                    //var dt = DateTime.ParseExact(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture), chekExist.PeriodFormat, CultureInfo.InvariantCulture);
                    var dt = DateTime.UtcNow.ToString(chekExist.PeriodFormat, CultureInfo.InvariantCulture);
                    generatedNum = chekExist.Prefix + sep1 + dt + sep2 +
                                   (chekExist.LastIndex + 1).ToString().PadLeft(length, '0'); //+ "_" + DateTime.UtcNow.Month + "_" + DateTime.UtcNow.Year;
                }
                else
                {
                    generatedNum = chekExist.Prefix + sep2 + (chekExist.LastIndex + 1).ToString().PadLeft(length, '0'); //+ "_" + DateTime.UtcNow.Month + "_" + DateTime.UtcNow.Year;
                }
                generatedNum = generatedNum.TrimStart(sep11);
                generatedNum = generatedNum.TrimStart(sep22);
            }
            return generatedNum;
        }


        public void SaveRecord()
        {
            unitOfWork.Commit();
        }
    }
}
