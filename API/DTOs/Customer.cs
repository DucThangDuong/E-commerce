namespace API.DTOs
{
    public class ReqGetCustomerProfile
    {
        public int customerId { get; set; }
    }

    public class ReqUpdateCustomerProfile
    {
        public string? Name { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
