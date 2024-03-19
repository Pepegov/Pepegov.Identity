using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class GetApplicationPermissionsRequestConsumer : IConsumer<GetApplicationPermissionsRequest>
{
    private readonly IMediator _mediator;

    public GetApplicationPermissionsRequestConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<GetApplicationPermissionsRequest> context)
    {
        var result = await _mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}