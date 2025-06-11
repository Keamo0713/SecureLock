namespace pleSecureDoc.Models
{
    public class Employee
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Guid AzurePersonId { get; set; }
    }
}
