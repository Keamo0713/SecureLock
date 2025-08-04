using System;

namespace pleSecureDoc.Models
{
    public class DocumentRequest
    {
        public string RequestId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public DateTime RequestTime { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovalTime { get; set; }
        public bool IsFaceConfirmed { get; set; }
    }
}
