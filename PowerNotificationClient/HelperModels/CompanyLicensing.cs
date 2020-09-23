using System;

namespace PowerNotificationClient.HelperModels
{
    public class CompanyLicensing
    {
        public string CompanyId { get; set; }
        public string DivisionId { get; set; }
        public string DepartmentId { get; set; }
        public string CustomerId { get; set; }
        public string RegCode { get; set; }
        public string LicensedItem { get; set; }
        public string LicensedCompanyName { get; set; }
        public int? Users { get; set; }
        public bool? Active { get; set; }
        public DateTime? LicenseStartDate { get; set; }
        public DateTime? LicenseEndDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public DateTime? SystemDate { get; set; }
        public string LockedBy { get; set; }
        public DateTime? LockTs { get; set; }
        public int Days { get; set; }
    }
}
