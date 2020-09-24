using PowerNotificationClient.HelperModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerNotificationClient
{
    public class SendNotification
    {
        private readonly List<RenewalNotificationDetails> details = new List<RenewalNotificationDetails>();
        public async Task<StatusMessage> SendNotificationasync()
        {
            StatusMessage status = new StatusMessage();
            try
            {
                var renewaldetails = await DataAccess.GetNotificationDetailsAsync();
                List<Task> tasks = new List<Task>();
                foreach (DataRow row in renewaldetails.Rows)
                {
                    tasks.Add(Task.Run(() => AddDetail(row)));
                }
                await Task.WhenAll(tasks);
                foreach (var renewal in details)
                {
                    var licenses = await DataAccess.GetLicenses(renewal.Days);
                    await Task.Run(() => Parallel.ForEach(licenses.AsEnumerable(), (singlelicense) =>
                      {
                          var customer = DataAccess.GetCustomerToNotifyAsync(singlelicense["CustomerID"].ToString()).GetAwaiter().GetResult();
                          var company = DataAccess.GetCompany();
                          var item = DataAccess.GetItem(singlelicense["LicensedItem"].ToString());
                          if (customer.Rows.Count > 0)
                          {
                              var task1 = Task.Run(() =>
                              {
                                  if ((bool)customer.Rows[0]["EmailAlert"] == true)
                                  {
                                      string address = company.Rows?[0]["CompanyAddress1"] + ", " + company.Rows?[0]["CompanyCity"] + ", " + company.Rows?[0]["CompanyState"] +
                                      ", " + company.Rows?[0]["CompanyCountry"];
                                      string subject = "Renewal Notification: " + item.Rows?[0]["ItemName"] + " - " + renewal.Days + " Days Notice";
                                      string body = "<div><p> Dear " + customer.Rows?[0]["CustomerName"] + ",</p> <br/></br/> <p> This is to notify you that your license for " + item.Rows?[0]["ItemName"] +
                                      " would be expiring in " + renewal.Days + " days." +
                                      "<br/><br/><p> You can renew on your dashboard on our website or contact us at " + company.Rows?[0]["CompanyEmail"].ToString() + " for more info.</p><br/><br/><p>Thanks.</p></div>";
                                      string path = Path.Combine(@"Setup/HtmlEmailTemplate.html");
                                      string FileContent = File.ReadAllText(path);
                                      string text = FileContent.Replace("{body}", body);
                                      text = text.Replace("{url}", "http://www.powersoft-solutions.org");
                                      text = text.Replace("{address}", address);
                                      text = text.Replace("'", "''");
                                      text = text.Replace("{phone}", company.Rows?[0]["CompanyPhone"].ToString());
                                      text = text.Replace("{email}", company.Rows?[0]["CompanyEmail"].ToString());
                                      text = text.Replace("{Date}", DateTime.Now.Year.ToString());
                                      DataAccess.SendNoticationMailAsync(subject, text, customer.Rows?[0]["CustomerEmail"].ToString()).GetAwaiter().GetResult();
                                  }
                              });
                              var task2 = Task.Run(() =>
                              {
                                  if ((bool)customer.Rows[0]["SMSAlert"] == true)
                                  {
                                      var webparameters = DataAccess.GetWebParameters();
                                      string body = "Renewal Notification: " + item.Rows?[0]["ItemName"] + " - " + renewal.Days + " Days Notice. Dear " + customer.Rows?[0]["CustomerName"] + ", This is to notify you that your license for " + item.Rows?[0]["ItemName"] +
                                      " would be expiring in " + renewal.Days + " days." +
                                      "You can renew on your dashboard on our website or contact us at " + company.Rows?[0]["CompanyEmail"].ToString() + " for more info.";
                                      DataAccess.sendSMSAsync(body, webparameters.Rows?[0]["SMSGatewayURL"].ToString(), webparameters.Rows?[0]["TextUserName"].ToString(), webparameters.Rows?[0]["TextPassword"].ToString(), customer.Rows?[0]["CustomerPhone"].ToString()).GetAwaiter().GetResult();
                                  }
                              });
                              Task.WhenAll(task1, task2).GetAwaiter().GetResult();
                          }
                      }));
                }
                status.status = "Success";
                status.message = "Success";
            }
            catch (Exception ex)
            {
                status.status = "Failed";
                status.message = ex.Message;
            }
            return await Task.FromResult(status);
        }
        private void AddDetail(DataRow row)
        {
            var detail = new RenewalNotificationDetails();
            detail.CompanyID = row["CompanyID"].ToString();
            detail.DivisionID = row["DivisionID"].ToString();
            detail.DepartmentID = row["DepartmentID"].ToString();
            detail.NotificationType = row["NotificationType"].ToString();
            detail.Days = (int)row["Days"];
            detail.Description = row["Description"].ToString();
            details.Add(detail);
        }
    }
}
