using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICustomerRepository
    {
        public Task AddAsync(string email,string password,string fullname);
        public Task SaveChangesAsync();
        public Task<Customer?> GetByEmailAsync(string email);
        public Task<bool> EmailExistsAsync(string email);
    }
}
