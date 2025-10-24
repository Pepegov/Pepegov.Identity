using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class GetApplicationPermissionsRequestConsumer(IMediator mediator) : IConsumer<GetApplicationPermissionsRequest>
{
    public async Task Consume(ConsumeContext<GetApplicationPermissionsRequest> context)
    {
        var result = await mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}