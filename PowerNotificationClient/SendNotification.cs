using PowerNotificationClient.HelperModels;
using System;
using System.Collections.Generic;
using System.Data;
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
                List<Task<Tuple<DataTable, int>>> task2 = new List<Task<Tuple<DataTable, int>>>();
                foreach (var renewal in details)
                {
                    task2.Add(DataAccess.GetLicenses(renewal.Days));
                }
                var licenses = await Task.WhenAll(task2);

                await Task.Run(() => Parallel.ForEach(licenses, (license) =>
                {
                    Parallel.ForEach(license.Item1.Rows.Cast<DataRow>(), (notifylicense) =>
                    {
                        var customer = DataAccess.GetCustomerToNotify(notifylicense["CustomerID"].ToString());
                        if (customer.Rows.Count > 0)
                        {
                            DataAccess.SendNoticationMail(customer.Rows[0], notifylicense, license.Item2);
                        }
                    });
                }));
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
