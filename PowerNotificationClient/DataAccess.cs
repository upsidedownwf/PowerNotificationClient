using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        public static DataTable ReturnDataTable(string query)
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

        public static SqlDataReader ReturnDataReader(string query)
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
        public static void SqlQuery(string query)
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
                 query = "Select * from NotificationSetupDetail where CompanyID='"+ CompanyID +"' and DivisionID='"+ DivisionID +
                    "' and DepartmentID='" + DepartmentID + "' and NotificationType='Renewal'";
            }
            catch (Exception ex)
            {

            }
            return await Task.Run(()=>ReturnDataTable(query));
        }
        public static async Task<Tuple<DataTable,int>> GetLicenses(int days)
        {
            try
            {
                query = "Select * from CompanyLicensing where CompanyID='" + CompanyID + "' and DivisionID='" + DivisionID +
                    "' and DepartmentID='" + DepartmentID + "' and Active=Cast(1 as bit) and DATEDIFF(DAY,GetDate(), LicenseEndDate)= Cast(" + days + " as int)";
            }
            catch (Exception ex)
            {

            }
            return Tuple.Create(await Task.Run(() => ReturnDataTable(query)), days);
        }
        public static DataTable GetCustomerToNotify(string customerId)
        {
            try
            {
                query = "Select * from CustomerInformation where  CompanyID='" + CompanyID + "' and DivisionID='" + DivisionID +
                    "' and DepartmentID='" + DepartmentID + "' and AccountStatus<> 'InActive' and CustomerID='" + customerId + "'";
            }
            catch (Exception ex)
            {

            }
            return ReturnDataTable(query);
        }
        public static void SendNoticationMail(DataRow customer, DataRow license, int days)
        {
            string subject = "wwww";
            string body = "xxxxxx";
            try
            {
                query = "Insert into _Mail_Send(Counter,SenderID, Recipient, Subject, Body, Attachment, CompanyID, DivisionID, DepartmentID) Values((select Top(1)Counter+1 from _mail_send order by Counter desc),'email@powersoft-solutions.org'," +
                    "'" + customer["CustomerEmail"].ToString() + "', '" + subject + "', '" + body + "', null, '" + CompanyID + "', '" + DivisionID + "', '" + DepartmentID + "')";
            }
            catch (Exception ex)
            {

            }
            SqlQuery(query);
        }
    }
}
