//-------------------------------------------------------------------------------------------------
// <copyright file="OfficeHelper.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Utilities
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Text;

    //using Microsoft.Office.Interop.Excel;
    using TestRunner.Models;
    using TestRunner.ViewModels;
    using System.Security.Principal;
    using System.Collections.Generic;
    using System.DirectoryServices;
    
    /// <summary>
    /// Office helper
    /// </summary>
    public static class OfficeHelper
    {
        #region Excel
        /// <summary>
        /// Export selection to Excel
        /// </summary>
        /// <param name="TestCasesModelICollectionView">Data source</param>
        internal static void ExportSelectionToExcel(ICollectionView TestCasesModelICollectionView)
        {
            /*
            Application xlApp = new Application();

            if (xlApp == null)
            {
                throw new ApplicationException("EXCEL could not be started. Check that your office installation and project references are correct.");
            }

            xlApp.Visible = true;

            Workbook wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet ws = (Worksheet)wb.Worksheets[1];

            if (ws == null)
            {
                throw new ApplicationException("Worksheet could not be created. Check that your office installation and project references are correct.");
            }

            // set header
            ws.Cells[1, 1] = "Priority";
            ws.Cells[1, 2] = "Name";
            ws.Cells[1, 3] = "Owner";
            ws.Cells[1, 4] = "Description";

            int i = 2;
            foreach (TestCase t in TestCasesModelICollectionView)
            {
                if (t.IsChecked)
                {
                    ws.Cells[i, 1] = t.Priority;
                    ws.Cells[i, 2] = t.Name;
                    ws.Cells[i, 3] = t.Owner;
                    ws.Cells[i, 4] = t.Description;
                    if (t.Custom1 != string.Empty)
                    {
                        ws.Cells[i, 5] = t.Custom1;
                    }

                    if (t.Custom1 != string.Empty)
                    {
                        ws.Cells[i, 6] = t.Custom2;
                    }

                    if (t.Custom1 != string.Empty)
                    {
                        ws.Cells[i, 7] = t.Custom3;
                    }

                    i++;
                }
            }
            */
        }

        #endregion

        #region Email

        internal static string ResolveAlias(string inputString)
        {
            inputString = inputString.Replace("@lseg.com", "");

            var dirEntry = new DirectoryEntry(string.Format("LDAP://{0}", "OU=UserAccounts,DC=fareast,DC=corp,DC=microsoft,DC=com"));
            var searcher = new DirectorySearcher(dirEntry)
            {
                Filter = string.Format("(&(objectCategory=person)(objectClass=user)(sAMAccountName={0}*))", inputString)
            };

            var resultCollection = searcher.FindAll();

            if (resultCollection.Count == 0)
            {
                searcher = new DirectorySearcher(dirEntry)
                {
                    Filter = string.Format("(&(objectCategory=person)(objectClass=user)(Displayname={0}*))", inputString)
                };
                
                resultCollection = searcher.FindAll();

                if (resultCollection.Count == 0)
                {
                    return "Not Found";
                }
                else
                {
                    if (resultCollection.Count > 1)
                        return "Invalid Alias";

                    var displayname = resultCollection[0].Properties["displayname"][0].ToString();
                    var mail = resultCollection[0].Properties["mail"][0].ToString();
                    //return resultCollection[0].GetDirectoryEntry().Name.Replace("CN=", "");
                    return mail;
                }
            }
            else
            {
                if (resultCollection.Count > 1)
                    return "Invalid Alias";
                var displayname = resultCollection[0].Properties["displayname"][0].ToString();
                var mail = resultCollection[0].Properties["mail"][0].ToString();
                //return resultCollection[0].GetDirectoryEntry().Name.Replace("CN=", "");
                return mail;
            }
        }

        /// <summary>
        /// Get log in alias
        /// </summary>
        /// <returns></returns>
        internal static string GetLocalAccount()
        {
            var aliasMail = "";
            char[] splitCh = { '\\' };
            string[] temp = WindowsIdentity.GetCurrent().Name.Split(splitCh, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length == 2)
            {
                aliasMail = String.Format("{0}@lseg.com", temp[1]);
            }
            return aliasMail;
        }

        /// <summary>
        /// Send email result to specific alias
        /// </summary>
        /// <param name="info"></param>
        internal static void SendMail(ICollectionView view, long totalTicks)
        {
            char[] splitCh = { '\\' };
            var mailFrom = "evai.sit@refinitiv.com";
            var mailCc = "bailin.wei@lseg.com";
            string[] temp = WindowsIdentity.GetCurrent().Name.Split(splitCh, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length == 2)
            {
                mailFrom = String.Format("{0}@lseg.com", temp[1]);
                if (mailCc.Contains("@"))
                {
                    if (!mailCc.Contains(String.Format(",{0}@lseg.com", temp[1])))
                    {
                        //mailCc += String.Format(",{0}@lseg.com", temp[1]);
                    }
                }
                else
                {
                    mailCc = String.Format("{0}@lseg.com", temp[1]);
                }
            }

            var info = new MailInfo();
            // TODO: change to multi styles
            info.MailContent = GetTestResultMailGreen(view, totalTicks);
            info.MailTo = mailFrom; // "bailin.wei@lseg.com";
            info.MailFrom = mailFrom;
            info.MailCC = mailCc;
            var countTotal = view.Cast<TestCase>().Count();
            var countPassed = view.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Passed);
            var countInconclusive = view.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Inconclusive);
            var passRate = countPassed / (float)(countTotal - countInconclusive);
            
            info.Subject = String.Format(TestCasesViewModel.EmailSubject, passRate.ToString("0.00%"));
            info.IsBodyHtml = true;
            //mail.AttachPath = path + "\\CheckFullMatch.sql"; this should be .trx file
            info.mailPriority = MailPriority.Normal;

            var myMail = new MailMessage();
            myMail.From = new MailAddress(info.MailFrom);
            myMail.To.Add(info.MailTo);
            //myMail.CC.Add(info.MailCC);
            //myMail.Bcc.Add("bailin.wei@lseg.com");
            myMail.Subject = info.Subject;
            myMail.SubjectEncoding = Encoding.UTF8;

            myMail.Body = info.MailContent;
            myMail.BodyEncoding = Encoding.UTF8;

            myMail.IsBodyHtml = info.IsBodyHtml;
            myMail.Priority = info.mailPriority;

            string attachPath = info.AttachPath;
            if (File.Exists(attachPath))
            {
                myMail.Attachments.Add(new Attachment(attachPath));
            }

            //var sender = new SmtpClient("smtphost", 25);
            //var sender = new SmtpClient("tfusnjpscsmtp1.tfn.com", 25);
            var sender = new SmtpClient("smtp.corp.internal", 25);
            //todo:check mail sender logic of vah
            sender.UseDefaultCredentials = true;
            //sender.Credentials = new NetworkCredential("test@gmail.com", "test");
            //sender.DeliveryMethod = SmtpDeliveryMethod.Network;
            sender.EnableSsl = true;

            try
            {
                sender.Send(myMail);
                LoggerViewModel.Log(string.Format("Sent a report to {0}", info.MailTo));
            }
            catch (Exception e)
            {
                var innerMessage = e.InnerException == null ? string.Empty : string.Format(" Inner exception: {0}", e.InnerException.Message);
                LoggerViewModel.Log(string.Format(
                    "Fail to send the report using SMTP server {0}:{1} from {2} to {3}: {4}.{5}",
                    sender.Host,
                    sender.Port,
                    info.MailFrom,
                    info.MailTo,
                    e.Message,
                    innerMessage));
            }
        }

        /// <summary>
        /// Sorted Inconclusive and failed tests ahead
        /// </summary>
        internal static List<TestCase> GetSortedList(ICollectionView originalTrxICollectionView)
        {
            List<TestCase> SortedList = new List<TestCase>();
            foreach (TestCase tc in originalTrxICollectionView)
            {
                SortedList.Add(tc);
            }

            return SortedList.OrderByDescending(x => x.State).ToList(); 
        }

        /// <summary>
        /// Email Template 1
        /// </summary>
        /// <returns></returns>
        internal static string GetTestResultMailBlack(ICollectionView originalTrxICollectionView)
        {
            List<TestCase> trxICollectionView = GetSortedList(originalTrxICollectionView);

            int rowIndex = 0;

            //foreach (DataGridViewRow dtgRow in gridReferance.Rows.Cast<DataGridViewRow>().Where(r => r.Visible == true && r.Cells[2].ToolTipText != ""))
            //{
            //    currentPatchesList.Add(dtgRow.Cells[1].Value.ToString());
            //}

            string logPath = String.Format("{0}\\{1}{2}", AppDomain.CurrentDomain.BaseDirectory, String.Format("{0:yyyyMM}", "testfoo"), "logfilename");
            //if (File.Exists(logPath))
            //{
            //    string[] lines = File.ReadAllLines(logPath);
            //    lastPatchesList = lines.ToList();
            //}

            //newPatchesList = (from p in currentPatchesList
            //                  where !lastPatchesList.Contains(p)
            //                  select p
            //                      ).ToList();

            //lostPatchesList = (from p in lastPatchesList
            //                   where !currentPatchesList.Contains(p)
            //                   select p
            //                      ).ToList();

            //if (newPatchesList.Count + lostPatchesList.Count > 0)
            //{
            //    m_IsPatchesDifferent = true;
            //}
            var strbd = new StringBuilder();

            strbd.AppendLine(@"
<html>
<head>
<style type='text/css'>
.iTableHeaderStyle {
   font-family: 'segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 18px;
   color: #726A67;
   vertical-align: middle;
   padding-top: 3px;
   padding-bottom: 3px
}
.summDataStyle {
   font-family: 'segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 16px;
   vertical-align: middle;
   padding-left: 10px;
   padding-right: 10px;
}
.dataStyle {
   font-family: 'segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 16px;
   vertical-align: middle;
   padding-left: 10px
}
.dataBoldStyle {
   font-family: 'segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 16px;
   font-weight: bold;
   text-align: right;
   vertical-align: middle;
}
.rColHeadStyle {
   border-width: 1px;
   border-color: #FFFFFF;
   background-color: #2D2E32;
   font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 14px;
   color: #FFFFFF;
   text-align: center;
   padding: 2px
}
.rColDataFirstStyle {
   border-style:  none solid solid solid;
   border-width: 1px;
   border-color: #FFFFFF;
   background-color: #3D3D3D;
   padding: 3px;
   font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 14px;
   color: #FFFFFF;
   text-align: center
}
.rColDataOtherStyle {
   border-style:  none solid solid none;
   border-width: 1px;
   border-color: #FFFFFF;
   background-color: #3D3D3D;
   padding: 3px;
   font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 14px;
   color: #FFFFFF;
   text-align: center
}
.rColErrorFirstStyle {
	border-style: none none solid solid;
	border-width: 1px;
	border-color: #FFFFFF;
	background-color: #CFCFCF;
	padding: 2px
}
.rColErrorSecondStyle {
	border-style: none solid solid none;
	border-width: 1px;
	border-color: #FFFFFF;
	background-color: #CFCFCF;
	text-align: left;
	padding: 2px;
	font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
	font-size: 13px;
	color: #000000
}
.legBlockStyle{
	font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
	font-size: 14px;
	font-weight: bold;
	color: #FFFFFF;
	vertical-align: middle;
	text-align: center;
	width: 34px;
	height: 34px
}
.legDescStyle{
	font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
	font-size: 14px;
	color: #808080;
	vertical-align: top
}
.legendStyle {
   font-family: 'segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 13px;
   color: #808080
}
</style>
<body>
<table style='border: 1px solid #FFE211; width: 1502px; background-color: #FFF6BD; font-family: segoe UI, Tahoma, Geneva, Verdana, sans-serif; font-size: 13px;'>
	<tr>
		<td style='height: 21px'>WARNING: The results in this report were sent by Test Runner automatically, any concern please contact application");
            strbd.AppendLine("launcher(" + WindowsIdentity.GetCurrent().Name + ") directly</td></tr></table><br/><br/>");

            strbd.AppendLine("<table cellpadding=\"0\" cellspacing=\"0\">");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("    <td colspan=\"5\" class=\"iTableHeaderStyle\">:: result summary</td>");
            strbd.AppendLine("</tr>");

            var countTotal = trxICollectionView.Cast<TestCase>().Count();
            var countAborted = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Aborted);
            var countFailed = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Failed);
            var countInconclusive = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Inconclusive);
            var countNotExecuted = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.NotExecuted);
            var countPassed = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Passed);
            var countPassedbutRunAborted = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.PassedButRunAborted);
            var countTimeout = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Timeout);
            var passRate = countPassed/(float)(countTotal - countInconclusive);

            var totalTicks = trxICollectionView.Cast<TestCase>().Sum(t => t.Duration == null ? 0 : (t.Duration.Value.Ticks));

            strbd.AppendLine("<tr>");
            strbd.AppendLine("    <td rowspan='6' style='color: #87BD46; font-family: segoe UI, Tahoma, Geneva, Verdana, sans-serif; font-size: 35px; padding-right: 5px; vertical-align: middle'>" + passRate.ToString("0.00%") + "</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Aborted:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>" + countAborted +"</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Not Executed:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>" + countNotExecuted + "</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Disconnected:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>0</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Not Runnable:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>0</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Error:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='color: #FE5815;'>0</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Passed But Run Aborted:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='color: #77BB44;'>" + countPassedbutRunAborted + "</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Passed:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='color: #77BB44;'>" + countPassed + "</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Timeout:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>" + countTimeout + "</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Failed:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='color: #FE5815;'>" + countFailed + "</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Warning:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='color: #FF9900;'>0</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Inconclusive:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>" + countInconclusive + "</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; font-weight: bold; padding: 0px'>Total:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style=''font-weight: bold;'>" + countTotal + "</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='dataStyle' style='padding-left: 30px;' colspan='5'>Total Duration:");
            //strbd.AppendLine("	<strong>0 day(s) 6 hour(s) 50 min(s) 16 sec(s)</strong></td>");
            //strbd.AppendLine("	<strong>" + string.Format("{0:dd} day(s) {1:hh} hour(s) {2:mm} min(s) {3:ss} sec(s)", summary.Day, summary.Hour, summary.Minute, summary.Second) + "</strong></td>");
            var seconds = (totalTicks / 10000000) % 60;
            var minutes = (totalTicks / 10000000) / 60;
            var hours = (totalTicks / 10000000) / (60 * 60);
            var days = (totalTicks / 10000000) / (60 * 60 * 12);
            strbd.AppendLine("	<strong>" + string.Format("{0} day(s) {1} hour(s) {2} min(s) {3} sec(s)", days, hours, minutes, seconds) + "</strong></td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("</table>");

            //strbd.AppendLine("    <p><span style=\"font-family: Calibri; font-size: 11pt\">");
            //strbd.AppendLine("        Eiffel Test Team Automation Report " + String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now) + "</span><br>");
            //strbd.AppendLine("<b><i><span style='font-size:9.0pt;font-family:\"Arial\",\"sans-serif\";");
            //strbd.AppendLine("color:white;background:green;mso-highlight:green'>This Notification is sent by");
            //strbd.AppendLine("Test Runner(" + Assembly.GetExecutingAssembly().GetName().Version + ") automatically, any concern please contact application");
            //strbd.AppendLine("launcher(" + WindowsIdentity.GetCurrent().Name + ") directly.</span></i></b></p>");
            //strbd.AppendLine("    <span style=\"font-family: Calibri; font-size: 11pt\"><u>Summary:</u></span>");
            //strbd.AppendLine("    <ul>");
            //strbd.AppendLine("        <li><span style=\"font-family: Calibri; font-size: 11pt\">Altogether in this detection, we have got " + currentPatchesList.Count.ToString() + " patch" + (currentPatchesList.Count > 1 ? "es" : "") + ".</span>");
            //strbd.AppendLine("        </li>");
            //strbd.AppendLine("        <li><span style=\"font-family: Calibri; font-size: 11pt\">" + newPatchesList.Count.ToString() + " new patch" + (newPatchesList.Count > 1 ? "es are" : " is") + " found, " + lostPatchesList.Count.ToString() + " primary patch" + (lostPatchesList.Count > 1 ? "es" : ""));
            //strbd.Append("            disappeared");
            //for (int i = 0; i < lostPatchesList.Count; i++)
            //{
            //    if (i == 0)
            //    {
            //        strbd.Append(" (");
            //    }

            //    if (i + 1 == lostPatchesList.Count)
            //    {
            //        strbd.AppendFormat("{0})", lostPatchesList[i]);
            //    }
            //    else
            //    {
            //        strbd.AppendFormat("{0}, ", lostPatchesList[i]);
            //    }
            //}
            //strbd.AppendLine(lostPatchesList.Count == 0 ? "." : ", maybe the ship date is changed.</span> </li>");
            //strbd.AppendLine("    </ul>");
            //strbd.AppendLine("    <p style=\"font-family: Calibri; font-size: 11pt\">");
            //strbd.AppendLine("        <u>Details:</u></p>");
            //strbd.AppendLine("        <p class=MsoNormal><span style='font-size:11.0pt;font-family:\"Calibri\",\"sans-serif\"'>" + String.Format("{0:MMMM, yyyy}", "Foo") + " test result(s).</span>");
            //strbd.AppendLine("        <span style='font-size:11.0pt;font-family:\"Calibri\",\"sans-serif\";");
            //strbd.AppendLine("        color:#1F497D'> <a href=\"file:///" + "ffffffooooo" + "\">" + "foooooooooo" + "</a>");
            //strbd.AppendLine("        </span><span style='font-size:11.0pt;font-family:\"Calibri\",\"sans-serif\"'><o:p></o:p></span></p>");

            strbd.AppendLine("    <br/><br/>");

            strbd.AppendLine("    <table cellspacing=\"0\" cellpadding=\"0\" style=\"width: 1502px\">");
            
            strbd.AppendLine("        <tr>");
            strbd.AppendLine("            <td colspan=\"10\" class=\"iTableHeaderStyle\">:: test result(s)</td>");
            strbd.AppendLine("        </tr>");
		
            strbd.AppendLine("        <tr>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 64px; border-style: none solid solid solid;\">Rerun</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 64px; border-style: none solid solid none;\">Priority</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"text-align: left; width: 800px; border-style: none solid solid none;\">Test Name</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 145px; border-style: none solid solid none;\">Owner</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 145px; border-style: none solid solid none;\">Result</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 64px; border-style: none solid solid none;\">Test Machine</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" colspan=\"6\" style=\"width: 204px; border-style: none solid solid none;\">Performance</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 80px; border-style: none solid solid none;\">Duration</td>");


            //strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"border: 1px solid #5F497A\"></td>")};
            //strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"border: 1px solid #5F497A\">");
            //strbd.AppendLine("                Priority");
            //strbd.AppendLine("            </td>");
            //strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"border: 1px solid #5F497A\">");
            //strbd.AppendLine("                Test Name");
            //strbd.AppendLine("            </td>");
            //strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"border: 1px solid #5F497A\">");
            //strbd.AppendLine("                Owner");
            //strbd.AppendLine("            </td>");
            //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
            //strbd.AppendLine("                Result");
            //strbd.AppendLine("            </td>");
            //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
            //strbd.AppendLine("                Test Machine");
            //strbd.AppendLine("            </td>");
            //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
            //strbd.AppendLine("                History");
            //strbd.AppendLine("            </td>");
            //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
            //strbd.AppendLine("                Duration");
            //strbd.AppendLine("            </td>");
            strbd.AppendLine("        </tr>");

            foreach (TestCase tc in trxICollectionView)
            {
                strbd.AppendLine("        <tr bgcolor=\"White\">");


                strbd.AppendLine("            <td class=\"rColDataFirstStyle\" style=\"width: 64px;\">" + (tc.RerunTimes > 0 ? tc.RerunTimes.ToString() : "") + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 64px;\">" + tc.Priority + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"text-align: left; width: 800px;\">" + tc.Name + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 145px;\">" + tc.Owner + "</td>");
                if (tc.State == TestCaseState.Passed)
                {
                    strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 145px; color: #77BB44;\">" + tc.State + "</td>");
                }
                else
                {
                    strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 145px; color: #FE5815;\">" + tc.State + "</td>");
                }
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 145px;\">" + tc.ClientMachineName + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #808080;\">" + "N" + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #808080;\">" + "N" + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #808080;\">" + "N" + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #808080;\">" + "N" + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #808080;\">" + "N" + "</td>");
                if (tc.State == TestCaseState.Passed)
                {
                    strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #77BB44;\">" + "P" + "</td>");
                }
                else
                {
                    strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #FE5815;\">" + "F" + "</td>");
                }
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 80px;\">" + string.Format("{0:mm:ss.fff}", tc.Duration) + "</td>");
                
                //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
                //strbd.AppendLine("                " + (++rowIndex).ToString());
                //strbd.AppendLine("            </td>");
                //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
                //strbd.AppendLine("                " + tc.Priority);
                //strbd.AppendLine("            </td>");
                //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
                //strbd.AppendLine("                " + tc.Name);
                //strbd.AppendLine("            </td>");
                //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
                //strbd.AppendLine("                " + tc.Owner);
                //strbd.AppendLine("            </td>");
                ////ServicePackageDistinction servicePackageDistionctionItem = (ServicePackageDistinction)dtgRow.Cells[6].Value;
                ////switch (servicePackageDistionctionItem)
                ////{
                ////    case ServicePackageDistinction.OnlySP1: strbd.Append(";background:#92D050;"); break;
                ////    case ServicePackageDistinction.OnlySP2: strbd.Append(";background:#548123;"); break;
                ////    case ServicePackageDistinction.BothSP1AndSP2: strbd.Append(";background:#74B230;"); break;
                ////    case ServicePackageDistinction.Unknown: break;
                ////    default: break;
                ////}
                ////strbd.AppendLine("\">");
                ////strbd.AppendLine("<p class=MsoNormal><span style='font-size:11.0pt;font-family:\"Calibri\",\"sans-serif\";color:white'>" + GetServicePackageDistinction(servicePackageDistionctionItem) + "<o:p></o:p></span></p>");
                ////strbd.AppendLine("            </td>");
                //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
                //strbd.AppendLine("                " + tc.State);
                //strbd.AppendLine("            </td>");
                ////strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
                ////strbd.AppendLine("                <a href=\"http://winseapps/segdr/ContentProposal.aspx?ID=" + dtgRow.Cells[0].Value.ToString() + "\">");
                ////strbd.AppendLine(System.IO.Path.GetFileName(dtgRow.Cells[2].ToolTipText) + "</a>");
                ////strbd.AppendLine("            </td>");
                //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
                //strbd.AppendLine("                " + tc.ClientMachineName);
                //strbd.AppendLine("            </td>");
                //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
                //strbd.AppendLine("                " );
                //strbd.AppendLine("            </td>");
                //strbd.AppendLine("            <td style=\"border: 1px solid #5F497A\">");
                //strbd.AppendLine("                " + string.Format("{0:mm:ss.ttt}", tc.Duration));
                //strbd.AppendLine("            </td>");
                strbd.AppendLine("        </tr>");
                if (tc.State!= TestCaseState.Passed)
                {
                    strbd.AppendLine("        <tr>");
                    strbd.AppendLine("            <td class=\"rColErrorFirstStyle\">");
                    strbd.AppendLine("                ");
                    strbd.AppendLine("            </td>");
                    strbd.AppendLine("            <td class=\"rColErrorSecondStyle\" colspan=\"12\">");
                    strbd.AppendLine("                " + tc.BriefErrorMessage);
                    strbd.AppendLine("            </td>");
                    strbd.AppendLine("        </tr>");
                }
            }
            strbd.AppendLine("    </table>");



            ////Reject Patches list
            //rowIndex = 0;
            //strbd.AppendLine("    <p style=\"font-family: Calibri; font-size: 11pt\">");
            //strbd.AppendLine("        <u>Reject Patches list:</u></p>");
            //strbd.AppendLine("    <table style=\"border: none; font-family: Calibri; font-size: 11pt; table-layout: auto;");
            //strbd.AppendLine("        border-collapse: collapse; padding-right: 10px; padding-left: 10px;\">");
            //strbd.AppendLine("        <tr style=\"background-color: #C0504D; font-weight: bold; color: #FFFFFF;\">");
            //strbd.AppendLine("            <td style=\"border: 1px solid #943634\"></td>");
            //strbd.AppendLine("            <td style=\"border: 1px solid #943634\">");
            //strbd.AppendLine("                KB Number");
            //strbd.AppendLine("            </td>");
            //strbd.AppendLine("            <td style=\"border: 1px solid #943634\">");
            //strbd.AppendLine("                Title");
            //strbd.AppendLine("            </td>");
            //strbd.AppendLine("            <td style=\"border: 1px solid #943634\">");
            //strbd.AppendLine("                Reject Reason");
            //strbd.AppendLine("            </td>");
            //strbd.AppendLine("            <td style=\"border: 1px solid #943634\">");
            //strbd.AppendLine("                Release Detail");
            //strbd.AppendLine("            </td>");
            //strbd.AppendLine("            <td style=\"border: 1px solid #943634\">");
            //strbd.AppendLine("                Ship Date");
            //strbd.AppendLine("            </td>");
            //strbd.AppendLine("        </tr>");
            ////foreach (DataGridViewRow dtgRow in gridReferance.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[5].Value != null).OrderBy(s => GetRejectReason(((List<RejectReason>)s.Cells[5].Value)[0])).ThenBy(s => s.Cells[1].Value.ToString()))
            ////{
            ////    if (newPatchesList.Contains(dtgRow.Cells[1].Value.ToString()))
            ////    {
            ////        strbd.AppendLine("        <tr bgcolor=\"Yellow\">");
            ////    }
            ////    else
            ////    {
            ////        strbd.AppendLine("        <tr>");
            ////    }
            ////    strbd.AppendLine("            <td style=\"border: 1px solid #943634\">");
            ////    strbd.AppendLine("                " + (++rowIndex).ToString());
            ////    strbd.AppendLine("            </td>");
            ////    strbd.AppendLine("            <td style=\"border: 1px solid #943634\">");
            ////    strbd.AppendLine("                " + dtgRow.Cells[1].Value.ToString());
            ////    strbd.AppendLine("            </td>");
            ////    strbd.AppendLine("            <td style=\"border: 1px solid #943634\">");
            ////    strbd.AppendLine("                " + dtgRow.Cells[3].Value.ToString());
            ////    strbd.AppendLine("            </td>");
            ////    strbd.AppendLine("            <td style=\"border: 1px solid #943634\">");
            ////    List<RejectReason> rejectReason = dtgRow.Cells[5].Value as List<RejectReason>;
            ////    if (rejectReason.Count == 1)
            ////    {
            ////        if (rejectReason[0] == RejectReason.NoPackageExists || rejectReason[0] == RejectReason.CABPackage || rejectReason[0] == RejectReason.X64PackageOnly)
            ////        {
            ////            strbd.AppendLine("<a href=\"http://winseapps/segdr/BugDetails.aspx?ID=" + dtgRow.Cells[2].Value.ToString() + "\">");
            ////            strbd.AppendLine(GetRejectReason(rejectReason[0]));
            ////            strbd.AppendLine("</a>");
            ////        }
            ////        else
            ////        {
            ////            strbd.AppendLine("                " + GetRejectReason(rejectReason[0]));
            ////        }
            ////    }
            ////    else
            ////    {
            ////        strbd.AppendLine("    <ul>");
            ////        foreach (RejectReason reason in rejectReason)
            ////        {
            ////            if (reason == RejectReason.NoPackageExists || reason == RejectReason.CABPackage || reason == RejectReason.X64PackageOnly)
            ////            {
            ////                strbd.AppendLine("<li>");
            ////                strbd.AppendLine("<a href=\"http://winseapps/segdr/BugDetails.aspx?ID=" + dtgRow.Cells[2].Value.ToString() + "\">");
            ////                strbd.AppendLine(GetRejectReason(reason));
            ////                strbd.AppendLine("</a>");
            ////                strbd.AppendLine("</li>");
            ////            }
            ////            else
            ////            {
            ////                strbd.AppendLine(String.Format("<li>{0}</li>", GetRejectReason(reason)));
            ////            }
            ////        }
            ////        strbd.AppendLine("    </ul>");
            ////    }
            ////    strbd.AppendLine("            </td>");
            ////    strbd.AppendLine("            <td style=\"border: 1px solid #943634\">");
            ////    strbd.AppendLine("                <a href=\"http://winseapps/segdr/ContentProposal.aspx?ID=" + dtgRow.Cells[0].Value.ToString() + "\">");
            ////    strbd.AppendLine("Release Details</a>");
            ////    strbd.AppendLine("            </td>");
            ////    strbd.AppendLine("            <td style=\"border: 1px solid #943634\">");
            ////    strbd.AppendLine("                " + dtgRow.Cells[7].Value.ToString());
            ////    strbd.AppendLine("            </td>");
            ////    strbd.AppendLine("        </tr>");
            ////}
            //strbd.AppendLine("    </table>");

            //strbd.AppendLine("    <p style=\"font-family: Calibri; font-size: 11pt; color:gray\">");
            //strbd.AppendLine("        You can also do some manual check here&nbsp;<a href=\"http://winseapps/segdr/MasterTracker.aspx\">http://winseapps/segdr/MasterTracker.aspx</a></p>");


            strbd.AppendLine("    <p style=\"font-family: Calibri; font-size: 11pt\">");
            strbd.AppendLine("        " + TestCasesViewModel.EmailSignature.Replace("\r\n", "<br/>") + "</p>");
            strbd.AppendLine("    <span style=\"font-size: 8.0pt; font-family: Arial; sans-serif; color: black;\"><b><i>");
            strbd.AppendLine("        This email message may contain confidential and proprietary information.&nbsp; Any");
            strbd.AppendLine("        unauthorized use is prohibited.&nbsp; If you are not the intended recipient, please");
            strbd.AppendLine("        contact the sender by reply email and destroy all copies of the original message.</i></b></span>");
            strbd.AppendLine("</body>");
            strbd.AppendLine("</html>");

            return strbd.ToString();
        }

        /// <summary>
        /// Email Template 2
        /// </summary>
        /// <returns></returns>
        internal static string GetTestResultMailGreen(ICollectionView originalTrxICollectionView, long totalTicks)
        {
            List<TestCase> trxICollectionView = GetSortedList(originalTrxICollectionView);

            int rowIndex = 0;
            
            string logPath = String.Format("{0}\\{1}{2}", AppDomain.CurrentDomain.BaseDirectory, String.Format("{0:yyyyMM}", "testfoo"), "logfilename");
            
            var strbd = new StringBuilder();

            strbd.AppendLine(@"
<html>
<head>
<style type='text/css'>
.iTableHeaderStyle {
   font-family: 'segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 18px;
   color: #726A67;
   vertical-align: middle;
   padding-top: 3px;
   padding-bottom: 3px
}
.summDataStyle {
   font-family: 'segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 16px;
   vertical-align: middle;
   padding-left: 10px;
   padding-right: 10px;
}
.dataStyle {
   font-family: 'segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 16px;
   vertical-align: middle;
   padding-left: 10px
}
.dataBoldStyle {
   font-family: 'segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 16px;
   font-weight: bold;
   text-align: right;
   vertical-align: middle;
}
.rColHeadStyle {
   border-width: 1px;
   border-color: #FFFFFF;
   background-color: #9BBB59;
   font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 14px;
   color: #FFFFFF;
   text-align: center;
   padding: 2px
}
.rColDataFirstStyle {
   border-style:  none solid solid solid;
   border-width: 1px;
   border-color: #FFFFFF;
   background-color: #EBF1DE;
   padding: 3px;
   font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 14px;
   color: #000000;
   text-align: center
}
.rColDataOtherStyle {
   border-style:  none solid solid none;
   border-width: 1px;
   border-color: #FFFFFF;
   background-color: #EBF1DE;
   padding: 3px;
   font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 14px;
   color: #000000;
   text-align: center
}
.rColErrorFirstStyle {
	border-style: none none solid solid;
	border-width: 1px;
	border-color: #FFFFFF;
	background-color: #FFFFCC;
	padding: 2px
}
.rColErrorSecondStyle {
	border-style: none solid solid none;
	border-width: 1px;
	border-color: #FFFFFF;
	background-color: #FFFFCC;
	text-align: left;
	padding: 2px;
	font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
	font-size: 13px;
	color: #000000
}
.legBlockStyle{
	font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
	font-size: 14px;
	font-weight: bold;
	color: #FFFFFF;
	vertical-align: middle;
	text-align: center;
	width: 34px;
	height: 34px
}
.legDescStyle{
	font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
	font-size: 14px;
	color: #808080;
	vertical-align: top
}
.legendStyle {
   font-family: 'segoe UI', Tahoma, Geneva, Verdana, sans-serif;
   font-size: 13px;
   color: #808080
}
</style>
<body>
<table style='border: 1px solid #FFE211; width: 1502px; background-color: #FFF6BD; font-family: segoe UI, Tahoma, Geneva, Verdana, sans-serif; font-size: 13px;'>
	<tr>
		<td style='height: 21px'>WARNING: The results in this report were sent by Test Runner automatically, any concern please contact application");
            strbd.AppendLine("launcher(" + WindowsIdentity.GetCurrent().Name + ") directly</td></tr></table><br/><br/>");

            strbd.AppendLine("<table cellpadding=\"0\" cellspacing=\"0\">");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("    <td colspan=\"5\" class=\"iTableHeaderStyle\">:: result summary</td>");
            strbd.AppendLine("</tr>");

            var countTotal = trxICollectionView.Cast<TestCase>().Count();
            var countAborted = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Aborted);
            var countFailed = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Failed);
            var countInconclusive = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Inconclusive);
            var countNotExecuted = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.NotExecuted);
            var countPassed = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Passed);
            var countPassedbutRunAborted = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.PassedButRunAborted);
            var countTimeout = trxICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Timeout);
            var passRate = countPassed / (float)(countTotal - countInconclusive);
            
            //var totalSeconds = trxICollectionView.Cast<TestCase>().Sum(t => t.Duration == null ? 0 : (t.Duration.Value.Second));
            if (totalTicks < 0)
            {
                totalTicks = trxICollectionView.Cast<TestCase>().Sum(t => t.Duration == null ? 0 : (t.Duration.Value.Ticks));
            }

            strbd.AppendLine("<tr>");
            strbd.AppendLine("    <td rowspan='6' style='color: #87BD46; font-family: segoe UI, Tahoma, Geneva, Verdana, sans-serif; font-size: 35px; padding-right: 5px; vertical-align: middle'>" + passRate.ToString("0.00%") + "</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Aborted:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>" + countAborted + "</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Not Executed:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>" + countNotExecuted + "</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Disconnected:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>0</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Not Runnable:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>0</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Error:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='color: #FE5815;'>0</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Passed But Run Aborted:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='color: #77BB44;'>" + countPassedbutRunAborted + "</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Passed:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='color: #77BB44;'>" + countPassed + "</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Timeout:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>" + countTimeout + "</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Failed:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='color: #FE5815;'>" + countFailed + "</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Warning:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='color: #FF9900;'>0</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; padding: 0px'>Inconclusive:</td>");
            strbd.AppendLine("	<td class='summDataStyle'>" + countInconclusive + "</td>");
            strbd.AppendLine("	<td class='summDataStyle' style='text-align:right; font-weight: bold; padding: 0px'>Total:</td>");
            strbd.AppendLine("	<td class='summDataStyle' style=''font-weight: bold;'>" + countTotal + "</td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("<tr>");
            strbd.AppendLine("	<td class='dataStyle' style='padding-left: 30px;' colspan='5'>Total Duration:");
            //var seconds = totalTicks % 60;
            //var minutes = (totalTicks / 60) % 60;
            //var hours = (totalTicks / (60 * 60)) % 24;
            TimeSpan testPassExecutedTime = new TimeSpan(totalTicks);
            //strbd.AppendLine("	<strong>" + string.Format("{0} hour(s) {1} min(s) {2} sec(s)", (int)hours, (int)minutes, (int)seconds) + "</strong></td>");
            strbd.AppendLine("	<strong>" + string.Format("{0} hour(s) {1} min(s) {2} sec(s)", testPassExecutedTime.Hours, testPassExecutedTime.Minutes, testPassExecutedTime.Seconds) + "</strong></td>");
            strbd.AppendLine("</tr>");
            strbd.AppendLine("</table>");

            strbd.AppendLine("    <br/><br/>");

            strbd.AppendLine("    <table cellspacing=\"0\" cellpadding=\"0\" style=\"width: 1502px\">");

            strbd.AppendLine("        <tr>");
            strbd.AppendLine("            <td colspan=\"10\" class=\"iTableHeaderStyle\">:: test result(s)</td>");
            strbd.AppendLine("        </tr>");

            strbd.AppendLine("        <tr>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 64px; border-style: none solid solid solid;\">Rerun</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 64px; border-style: none solid solid none;\">Priority</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"text-align: left; width: 800px; border-style: none solid solid none;\">Test Name</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 145px; border-style: none solid solid none;\">Owner</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 145px; border-style: none solid solid none;\">Result</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 64px; border-style: none solid solid none;\">Test Machine</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" colspan=\"6\" style=\"width: 204px; border-style: none solid solid none;\">Performance</td>");
            strbd.AppendLine("            <td class=\"rColHeadStyle\" style=\"width: 80px; border-style: none solid solid none;\">Duration</td>");

            strbd.AppendLine("        </tr>");

            foreach (TestCase tc in trxICollectionView)
            {
                strbd.AppendLine("        <tr bgcolor=\"White\">");


                strbd.AppendLine("            <td class=\"rColDataFirstStyle\" style=\"width: 64px;\">" + (tc.RerunTimes > 0 ? tc.RerunTimes.ToString() : "") + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 64px;\">" + tc.Priority + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"text-align: left; width: 800px;\">" + tc.Name + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 145px;\">" + tc.Owner + "</td>");
                if (tc.State == TestCaseState.Passed)
                {
                    strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 145px; color: #77BB44;\">" + tc.State + "</td>");
                }
                else
                {
                    if(tc.State == TestCaseState.Timeout)
                    {
                        strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 145px; color: #00B0F0;\">" + tc.State + "</td>");    
                    }
                    else if (tc.State == TestCaseState.Inconclusive)
                    {
                        strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 145px; color: #BFBFBF;\">" + tc.State + "</td>");    
                    }
                    else
                    {
                        strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 145px; color: #FE5815;\">" + tc.State + "</td>");    
                    }
                }
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 145px;\">" + tc.ClientMachineName + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #BFBFBF; color:#FFFFFF;\">" + "N" + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #BFBFBF; color:#FFFFFF;\">" + "N" + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #BFBFBF; color:#FFFFFF;\">" + "N" + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #BFBFBF; color:#FFFFFF;\">" + "N" + "</td>");
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #BFBFBF; color:#FFFFFF;\">" + "N" + "</td>");
                if (tc.State == TestCaseState.Passed)
                {
                    strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #77BB44; color:#FFFFFF;\">" + "P" + "</td>");
                }
                else if (tc.State == TestCaseState.Inconclusive)
                {
                    strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #FFFFCC; color:#FFFFFF;\">" + "I" + "</td>");
                }
                else
                {
                    strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 34px; font-size: 13px; background-color: #FE5815; color:#FFFFFF;\">" + "F" + "</td>");
                }
                strbd.AppendLine("            <td class=\"rColDataOtherStyle\" style=\"width: 80px;\">" + string.Format("{0:mm:ss.fff}", tc.Duration) + "</td>");

                strbd.AppendLine("        </tr>");
                if (tc.State != TestCaseState.Passed)
                {
                    strbd.AppendLine("        <tr>");
                    strbd.AppendLine("            <td class=\"rColErrorFirstStyle\">");
                    strbd.AppendLine("                ");
                    strbd.AppendLine("            </td>");
                    strbd.AppendLine("            <td class=\"rColErrorSecondStyle\" colspan=\"12\">");
                    //strbd.AppendLine("                :: Error Message:<br>" + tc.BriefErrorMessage + "<br><br>:: Standard Console output:<br>" + tc.StdOut.Replace("\r\n", "<br>"));
                    strbd.AppendLine("                " + tc.BriefErrorMessage);
                    strbd.AppendLine("            </td>");
                    strbd.AppendLine("        </tr>");
                }
            }
            strbd.AppendLine("    </table>");

            strbd.AppendLine("    <p style=\"font-family: Calibri; font-size: 11pt\">");
            strbd.AppendLine("        " + TestCasesViewModel.EmailSignature.Replace("\r\n", "<br/>") + "</p>");
            strbd.AppendLine("    <span style=\"font-size: 8.0pt; font-family: Arial; sans-serif; color: black;\"><b><i>");
            strbd.AppendLine("        This email message may contain confidential and proprietary information.&nbsp; Any");
            strbd.AppendLine("        unauthorized use is prohibited.&nbsp; If you are not the intended recipient, please");
            strbd.AppendLine("        contact the sender by reply email and destroy all copies of the original message.</i></b></span>");
            strbd.AppendLine("</body>");
            strbd.AppendLine("</html>");

            return strbd.ToString();
        }
        #endregion
    }

    public struct MailInfo
    {
        public string MailFrom;
        public string MailTo;
        public string MailCC;
        public string MailContent;
        public string AttachPath;
        public string Subject;
        public bool IsBodyHtml;
        public MailPriority mailPriority;
    };
}
