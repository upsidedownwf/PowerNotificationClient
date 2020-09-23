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
            //var details = new List<RenewalNotificationDetails>();
            try
            {
                var renewaldetails = await DataAccess.GetNotificationDetailsAsync();
                List<Task> tasks = new List<Task>();
                foreach (DataRow row in renewaldetails.Rows)
                {
                    tasks.Add(Task.Run(() => AddDetail(row)));
                }
                await Task.WhenAll(tasks);
                List<Task<DataTable>> task2 = new List<Task<DataTable>>();
                var NotifyLicenses = new List<CompanyLicensing>();
                foreach (var renewal in details)
                {
                    var licenses = await DataAccess.GetLicenses(renewal.Days);
                    await Task.Run(() => Parallel.ForEach(licenses.AsEnumerable(), (singlelicense) =>
                      {
                          var license = new CompanyLicensing();
                          license.CustomerId = singlelicense["CustomerID"].ToString();
                          license.LicenseStartDate = (DateTime)singlelicense["LicenseEndDate"];
                          license.LicenseEndDate = (DateTime)singlelicense["LicenseEndDate"];
                          license.LicensedCompanyName = singlelicense["LicensedCompanyName"].ToString();
                          license.LicensedItem = singlelicense["LicensedItem"].ToString();
                          license.RegCode = singlelicense["RegCode"].ToString();
                          license.Days = renewal.Days;
                          NotifyLicenses.Add(license);
                          Task.Run(() => Parallel.ForEach(NotifyLicenses, (notifylicense) =>
                          {
                              if (license != null)
                              {
                                  var customer = DataAccess.GetCustomerToNotifyAsync(notifylicense.CustomerId).GetAwaiter().GetResult();
                                  var company = DataAccess.GetCompany();
                                  if (customer.Rows.Count > 0)
                                  {
                                      if ((bool)customer.Rows[0]["EmailAlert"] == true)
                                      {
                                          var item = DataAccess.GetItem(license.LicensedItem);
                                          string address = company.Rows?[0]["CompanyAddress1"] + ", " + company.Rows?[0]["CompanyCity"] + ", " + company.Rows?[0]["CompanyState"] +
                                          ", " + company.Rows?[0]["CompanyCountry"];
                                          string subject = "notify";
                                          string body = "Renew Abeg";

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
                                  }
                              }
                          })).GetAwaiter().GetResult();
                      }));
                    ;
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
