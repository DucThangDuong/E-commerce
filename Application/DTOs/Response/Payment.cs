using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Response
{
    public record CreatePaymentResponse
    {
        public int OrderId { get; init; }
        public string? PaymentUrl { get; init; }
        public string Message { get; init; } = null!;
    }
}
