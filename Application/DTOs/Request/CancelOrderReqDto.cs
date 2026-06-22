namespace Application.DTOs.Request
{
    public class CancelOrderReqDto
    {
        public int OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
