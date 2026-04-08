using Application.DTOs.Response;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICustomerRepository
    {
        Task AddAsync(string email, string password, string fullname);
        Task AddAsync(Customer customer);
        Task<Customer?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<int> UpdateCustomerProfileAsync(int customerId, string name, string? phoneNumber, string? address, CancellationToken ct = default);
    }
}
