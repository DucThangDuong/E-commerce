namespace Application.DTOs.Response
{
    public class ResCancellationReasonDto
    {
        public int ReasonId { get; set; }
        public string Code { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
