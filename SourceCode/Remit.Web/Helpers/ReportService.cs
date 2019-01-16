using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using System.Reflection;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Office.Interop.Excel;
using System.Web.Mvc;
using System.Configuration;
using System.Net;
using Remit.Web.Controllers;
using System.Collections;
using Remit.Service.Enums;


namespace Remit.Web.Helpers
{
    public class ReportService : Controller
    {
        //public readonly IReportConfigurationService reportConfigurationService;
        public ReportService()
        {
        }

        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (dividend - modulo) / 26;
            }
            return columnName;
        }

    }
}
