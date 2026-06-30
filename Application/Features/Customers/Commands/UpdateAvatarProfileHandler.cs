using Application.Common;
using Application.Interfaces;
using Application.IServices;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Customers.Commands
{
    public record UpdateAvatarProfileCommand(IFormFile file, int userId) : IRequest<Result<string>>;
    public class UpdateAvatarProfileHandler : IRequestHandler<UpdateAvatarProfileCommand, Result<string>>
    {
        private readonly IBlobService _blobService;
        private readonly ICustomerRepository _customerRepository;
        public UpdateAvatarProfileHandler(IBlobService blobService, ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
            _blobService = blobService;
        }
        public async Task<Result<string>> Handle(UpdateAvatarProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string newAvatarUrl = await _blobService.UploadImageAsync(request.file, "avatar");
                int result = await _customerRepository.UpdateAvatarProfileAsync(request.userId, newAvatarUrl);
                if (result == 0)
                {
                    return Result<string>.Failure("Failed to update avatar profile.");
                }
                return Result<string>.Success(newAvatarUrl);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(ex.Message);
            }
        }
    }
}
