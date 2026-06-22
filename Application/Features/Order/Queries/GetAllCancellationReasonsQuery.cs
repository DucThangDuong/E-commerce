using Application.Common;
using Application.DTOs.Response;
using MediatR;

namespace Application.Features.Order.Queries
{
    public record GetAllCancellationReasonsQuery() : IRequest<Result<List<ResCancellationReasonDto>>>;
}
