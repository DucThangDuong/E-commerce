namespace API.DTOs
{


    public class ReqUpdateCustomerName
    {
        public string Name { get; set; } = string.Empty;
    }

    public class ReqUpdateCustomerPhone
    {
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class ReqUpdateCustomerAddress
    {
        public string Address { get; set; } = string.Empty;
    }

    public class ReqUpdateCustomerPassword
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
    public class ResUpdateAvatarProfile
    {
        public IFormFile? AvatarFile { get; set; }
    }
}
