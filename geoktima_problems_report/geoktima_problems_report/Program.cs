using Mono.Security.X509;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace geoktima_problems_report
{
    class Program
    {
        static void Main(string[] args)
        {
            if (DateTime.Now.DayOfWeek.ToString() != "Saturday" && DateTime.Now.DayOfWeek.ToString() != "Sunday")
            {
                MyClass myclass = new MyClass();
                myclass.currentDate = DateTime.Now.ToString("yyyy-M-dd");
                String toDayDir = Directory.GetCurrentDirectory() + "\\" + myclass.currentDate;
                for (int i = 0; i < myclass.databaseList.Count; i++)
                {
                    myclass.currentDataBase = myclass.databaseList[i];
                    myclass.reportDir = toDayDir + "\\" + myclass.currentDataBase + "_reports";
                    if (!Directory.Exists(myclass.reportDir))
                    {
                        try
                        {
                            Directory.CreateDirectory(myclass.reportDir);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message.ToString());
                        }
                    }
                    myclass.databaseReadAndWriteToCsvs();
                }
                StreamWriter logFileSW = new StreamWriter(toDayDir + "\\logs.txt");
                logFileSW.Write("ERRORS: " + myclass.errorLogsString + "\n\n\n REPORT:" + myclass.reportLogsString);
                logFileSW.Close();
                if (!File.Exists(myclass.currentDate + ".zip"))
                {
                    myclass.CreateZip(toDayDir, Directory.GetCurrentDirectory() + "\\" + myclass.currentDate + ".zip");
                }
                try
                {
                    StreamReader srEmailTo = new StreamReader(Directory.GetCurrentDirectory() + "\\emailTo.txt");
                    String[] emailsTo = srEmailTo.ReadToEnd().Replace("\r", "").Split('\n');
                    srEmailTo.Close();
                    for (int emailIndex = 0; emailIndex < emailsTo.Length; emailIndex++)
                    {
                        System.Net.Mail.Attachment attachment;
                        attachment = new System.Net.Mail.Attachment(myclass.currentDate + ".zip");
                        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                        String emailFrom = "test@test.gr", password = "test";
                        SmtpClient SmtpServer = new SmtpClient("test.gr");
                        var mail = new MailMessage();
                        mail.From = new MailAddress(emailFrom);
                        mail.To.Add(emailsTo[emailIndex]);
                        mail.Subject = "Geoktima_problems_reports";
                        mail.IsBodyHtml = true;
                        string htmlBody;
                        htmlBody = "Geoktima_problems_reports for " + DateTime.Now.ToString("dd-M-yyyy");
                        mail.Body = htmlBody;
                        mail.Attachments.Add(attachment);
                        SmtpServer.Port = 587;
                        SmtpServer.UseDefaultCredentials = false;
                        SmtpServer.Credentials = new System.Net.NetworkCredential(emailFrom, password);
                        SmtpServer.EnableSsl = true;
                        SmtpServer.Send(mail);
                    }
                }
                catch (Exception ex)
                {
                    myclass.errorLogsString = myclass.errorLogsString + " Αδυναμία αποστολής email. " + ex.Message + "\n";
                }
                myclass.deleteOldReports();
            }
        }







    }
}
