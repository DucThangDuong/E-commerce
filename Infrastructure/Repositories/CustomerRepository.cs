using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CustomerRepository:ICustomerRepository
    {
        private readonly EcommerceOrderSystemContext _context;
        public CustomerRepository(EcommerceOrderSystemContext context) { 
            _context = context;
        }

        public async Task AddAsync(string email, string password, string fullname)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            string username = email.Substring(0, email.LastIndexOf('@'));
            Customer? newuser = await _context.Customers.FirstOrDefaultAsync(e => e.Email == email);
            if (newuser != null)
            {
                newuser.PasswordHash = passwordHash;
                newuser.CustomAvatar = "default-avatar.jpg";
            }
            else
            {
                var newUser = new Customer
                {
                    Name = fullname,
                    Email = email,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow,
                    Role = "User",
                    IsActive = true,
                    LoginProvider = "Custom"
                };
                _context.Customers.Add(newUser);
            }
        }
        public async Task<Customer?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Customers.AsNoTracking().FirstOrDefaultAsync(e => e.RefreshToken == refreshToken);
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Customers.AnyAsync(c => c.Email == email);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }
    }
}
