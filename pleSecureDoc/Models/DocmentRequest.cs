using System;

namespace pleSecureDoc.Models
{
    public class DocumentRequest
    {
        public string RequestId { get; set; }
        public string EmployeeId { get; set; }
        public string DocumentName { get; set; }
        public DateTime RequestTime { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovalTime { get; set; }
        public bool IsFaceConfirmed { get; set; }
    }
}
