using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerNotificationClient.HelperModels
{
    public class RenewalNotificationDetails
    {
        public string CompanyID { get; set; }
        public string DivisionID { get; set; }
        public string DepartmentID { get; set; }
        public string NotificationType { get; set; }
        public int Days { get; set; }
        public string Description { get; set; }
    }
}
