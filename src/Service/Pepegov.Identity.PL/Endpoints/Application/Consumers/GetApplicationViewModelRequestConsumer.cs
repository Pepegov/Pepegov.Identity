using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class GetApplicationViewModelRequestConsumer(IMediator mediator) : IConsumer<GetApplicationViewModelRequest>
{
    public async Task Consume(ConsumeContext<GetApplicationViewModelRequest> context)
    {
        var result = await mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}