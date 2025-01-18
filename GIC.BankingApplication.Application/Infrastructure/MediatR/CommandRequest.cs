using GIC.BankingApplication.Application.Infrastructure.Response;
using MediatR;

namespace GIC.BankingApplication.Application.Infrastructure.MediatR;

public record CommandRequest : IRequest<IResponse>;