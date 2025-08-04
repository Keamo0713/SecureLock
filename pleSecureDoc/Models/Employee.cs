using System;

namespace pleSecureDoc.Models
{
    public class Employee
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid AzurePersonId { get; set; }
    }
}
