using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using System.Reflection;
using System.Diagnostics;


namespace Remit.LoggerService
{
    public class LoggingService 
    {
        private ILog logger;
        
        public string UserId { get; set; }
       
        #region constructor
        public LoggingService()
        {
            logger = LogManager.GetLogger(this.GetType());
        }

        public LoggingService(Type type)
        {
            logger = LogManager.GetLogger(type);
        }

        public LoggingService(string loggerName)
        {
            logger = LogManager.GetLogger(loggerName);
        }
        #endregion

        public void Info(string message)
        {
            logger.Info(message);
        }
        
        public void Warn(string message)
        {
            logger.Warn(message);
        }

        public void Debug(string message)
        {
            logger.Debug(message);
        }
        
        public void Error(string message)
        {
            logger.Error(message);
        }
       
        public void Error(string message, Exception ex)
        {
            logger.Error(message, ex);
        }
       
        public void Fatal(string message)
        {
            logger.Fatal(message);
        }
        

        public void WriteActionLog(Int32 Who, DateTime When, String AffectedRecordId, String What, String ActionCRUD, String Entity, int SubModuleItemId = 0)
        {
            try
            {
                log4net.GlobalContext.Properties["Who"] = Who;
                log4net.GlobalContext.Properties["When"] = When;
                log4net.GlobalContext.Properties["AffectedRecordId"] = AffectedRecordId;
                log4net.GlobalContext.Properties["What"] = What;
                log4net.GlobalContext.Properties["ActionCRUD"] = ActionCRUD;
                log4net.GlobalContext.Properties["Entity"] = Entity;
                //log4net.GlobalContext.Properties["SubModuleItemId"] = SubModuleItemId;
                logger.Info("");
            }
            catch (Exception ) {
            //
            }
        }

        
    }

}