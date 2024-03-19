using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class GetApplicationViewModelRequestConsumer : IConsumer<GetApplicationViewModelRequest>
{
    private readonly IMediator _mediator;

    public GetApplicationViewModelRequestConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<GetApplicationViewModelRequest> context)
    {
        var result = await _mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}