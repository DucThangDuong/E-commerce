using Application.DTOs.Services;
using Microsoft.AspNetCore.Http;
using System;

namespace Application.IServices
{

    public interface IVnPayService
    {
        string CreatePaymentUrl(int orderId, decimal amount, string ipAddress);
        ResVnPayDTO PaymentCallback(IQueryCollection collections);
        bool ValidateSignature(IQueryCollection collections);
    }
}
