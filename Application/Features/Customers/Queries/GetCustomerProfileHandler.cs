using Application.Common;
using Application.DTOs.Response;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Customers.Queries
{
    public record GetCustomerProfileQuery(int customerId);
    public class GetCustomerProfileHandler : IQueryHandler<GetCustomerProfileQuery, ResCustomerPrivate>
    {
        public readonly IUnitOfWork _context;
        public GetCustomerProfileHandler(IUnitOfWork context) { 
            _context = context;
        }
        public async Task<Result<ResCustomerPrivate>> HandleAsync(GetCustomerProfileQuery query, CancellationToken ct = default)
        {
            try
            {

                var customer = await _context.Context.Customers
                    .AsNoTracking()
                    .Where(x => x.CustomerId == query.customerId)
                    .Select(x => new ResCustomerPrivate
                    {
                        avatarUrl = x.CustomAvatar,
                        email = x.Email,
                        id = x.CustomerId,
                        name = x.Name
                    })
                    .FirstOrDefaultAsync(ct);
                if (customer == null)
                {
                    return Result<ResCustomerPrivate>.Failure("Not found", 404);
                }
                return Result<ResCustomerPrivate>.Success(customer);
            }
            catch (Exception ex) {
                return Result<ResCustomerPrivate>.Failure(ex.Message, 400);
            }
        }
    }
}
