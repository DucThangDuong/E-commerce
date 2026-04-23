namespace API.DTOs
{
    public class ReqGetCustomerProfile
    {
        public int customerId { get; set; }
    }

    public class ReqUpdateCustomerProfile
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
