using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace Remit.Web.Helpers
{
    public class EmailUtility
    {
        public static bool SendMail(List<string> receipients, List<string> carbonCopies, string subject, string body)
        {
            bool mailStatus = true;
           
            //string str = @System.Configuration.ConfigurationManager.AppSettings["mail:SourceUrl"];
            //string str1 = "<a href=" + str + ">Click here</a>";

            //string siteUrl = @System.Configuration.ConfigurationManager.AppSettings["mail:SiteUrl"];
            //siteUrl = string.Format(siteUrl, str1);
            //siteUrl = siteUrl.Replace("/r/n", "<br>");

            string sender = @System.Configuration.ConfigurationManager.AppSettings["mail:Sender"];
            sender = sender.Replace("/r/n", "<br>");

            MailMessage m = new MailMessage();
            SmtpClient sc = new SmtpClient();
            //m.From = new MailAddress("esharif21@gmail.com"); //set from web.config
            m.Subject = subject;
            m.IsBodyHtml = true;
            m.Body = body + "<br>" + "<br>" + sender;
            //sc.Credentials = info@accentechltd.com

            //m.Sender = new MailAddress(sender);

            //sc.Host="smtp.gmail.com";
            //int port = 587;

            sc.Host = "smtpout.asia.secureserver.net";//sc.Host; //"smtp.gmail.com";
            sc.Port = 25;//sc.Port; //= 587;
            //sc.Credentials = new System.Net.NetworkCredential("esharif21@gmail.com", "password"); //set from web.config

            sc.EnableSsl = true;
            try
            {
                foreach (var aReceipient in receipients)
                    m.To.Add(aReceipient); //to
                if (carbonCopies != null)
                    foreach (var aCarbonCopy in carbonCopies)
                        m.CC.Add(aCarbonCopy);
                sc.Send(m);
            }
            catch (Exception)
            {
                mailStatus = false;
            }
            return mailStatus;
        }
    }
}