using PowerNotificationClient.HelperModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PowerNotificationClient
{
    static class DataAccess
    {
        private static SqlConnection conn;
        private static SqlCommand sqlCmd;
        private static SqlDataAdapter _sqlDta;
        private static DataTable _dtb;
        private static SqlDataReader dataReader;
        private static readonly string path = Path.Combine(@"Setup/ConfigFile.txt");
        private static readonly string FileContent = File.ReadAllText(path);
        static char[] delimiter = new char[] { '|' };
        static string[] FileContentArray = FileContent.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
        private static readonly string _conn = FileContentArray[0];
        private static readonly string CompanyID = FileContentArray[1];
        private static readonly string DivisionID = FileContentArray[2];
        private static readonly string DepartmentID = FileContentArray[3];
        private static string query = string.Empty;
        private static HttpClient client;

        private static DataTable ReturnDataTable(string query)
        {
            using (conn = new SqlConnection(_conn))
            {
                using (sqlCmd = new SqlCommand(query, conn))
                {
                    _sqlDta = new SqlDataAdapter(sqlCmd);
                    _dtb = new DataTable();
                    _sqlDta.Fill(_dtb);
                    return _dtb;
                }
            }
        }

        private static SqlDataReader ReturnDataReader(string query)
        {
            using (conn = new SqlConnection(_conn))
            {
                conn.Open();
                using (sqlCmd = new SqlCommand(query, conn))
                {
                    dataReader = sqlCmd.ExecuteReader();
                    return dataReader;
                }
            }
        }
        private static void SqlQuery(string query)
        {
            using (conn = new SqlConnection(_conn))
            {
                conn.Open();
                using (sqlCmd = new SqlCommand(query, conn))
                {
                    sqlCmd.ExecuteNonQuery();
                }
            }
        }
        public static async Task<DataTable> GetNotificationDetailsAsync()
        {
            try
            {
                query = "Select * from NotificationSetupDetail where CompanyID='" + CompanyID + "' and DivisionID='" + DivisionID +
                   "' and DepartmentID='" + DepartmentID + "' and NotificationType='Renewal'";
            }
            catch (Exception ex)
            {

            }
            return await Task.Run(() => ReturnDataTable(query));
        }
        public static async Task<DataTable> GetLicenses(int days)
        {
            try
            {
                query = "Select * from CompanyLicensing where CompanyID='" + CompanyID + "' and DivisionID='" + DivisionID +
                    "' and DepartmentID='" + DepartmentID + "' and Active=Cast(1 as bit) and DATEDIFF(DAY,GetDate(), LicenseEndDate= Cast(" + days + " as int)";
            }
            catch (Exception ex)
            {

            }
            return await Task.Run(() => ReturnDataTable(query));
        }
        public static async Task<DataTable> GetCustomerToNotifyAsync(string customerId)
        {
            try
            {
                query = "Select * from CustomerInformation where  CompanyID='" + CompanyID + "' and DivisionID='" + DivisionID +
                    "' and DepartmentID='" + DepartmentID + "' and AccountStatus<> 'InActive' and CustomerID='" + customerId + "'";
            }
            catch (Exception ex)
            {

            }
            return await Task.Run(() => ReturnDataTable(query));
        }
        public static async Task SendNoticationMailAsync(string subject, string body, string customeremail)
        {
            try
            {
                query = "Insert into _Mail_Send(SenderID, Recipient, Subject, Body, Attachment, CompanyID, DivisionID, DepartmentID) Values('email@powersoft-solutions.org'," +
                    "'" + customeremail + "', '" + subject + "', '" + body + "', null, '" + CompanyID + "', '" + DivisionID + "', '" + DepartmentID + "')";
            }
            catch (Exception ex)
            {

            }
            await Task.Run(() => SqlQuery(query));
        }
        public static DataTable GetCompany()
        {
            try
            {
                query = "Select * from Companies where  CompanyID='" + CompanyID + "' and DivisionID='" + DivisionID +
                    "' and DepartmentID='" + DepartmentID + "'";
            }
            catch (Exception ex)
            {

            }
            return ReturnDataTable(query);
        }
        public static DataTable GetWebParameters()
        {
            try
            {
                query = "Select * from WebParameters where  CompanyID='" + CompanyID + "' and DivisionID='" + DivisionID +
                    "' and DepartmentID='" + DepartmentID + "'";
            }
            catch (Exception ex)
            {

            }
            return ReturnDataTable(query);
        }
        public static DataTable GetItem(string LicensedItem)
        {
            try
            {
                query = "Select * from InventoryItems where  CompanyID='" + CompanyID + "' and DivisionID='" + DivisionID +
                    "' and DepartmentID='" + DepartmentID + "' and ItemID='" + LicensedItem + "'";
            }
            catch (Exception ex)
            {

            }
            return ReturnDataTable(query);
        }
        public static async Task sendSMSAsync(string vmessage, string SMSGatewayURL, string TextUserName, string TextPassword, string CustomerPhone)
        {
            try
            {
                var uri = SMSGatewayURL + "?onweremail=" + TextUserName + "&subacctpwd=" + TextPassword + "&message=" + vmessage + "&sender=" + CompanyID + "&sendto=" + CustomerPhone + "&msgtype=0 ";

                client = new HttpClient();
                var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsStringAsync();
                Console.WriteLine(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
